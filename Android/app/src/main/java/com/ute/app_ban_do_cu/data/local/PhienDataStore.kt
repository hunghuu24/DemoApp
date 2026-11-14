package com.ute.app_ban_do_cu.data.local

import android.content.Context
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import androidx.datastore.preferences.core.PreferenceDataStoreFactory
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import com.google.gson.Gson
import com.google.gson.JsonParseException
import com.ute.app_ban_do_cu.domain.model.PhienDto
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.io.File
import java.security.GeneralSecurityException
import java.security.KeyStore
import java.util.concurrent.atomic.AtomicReference
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

data class PhienLuu(val accessToken: String, val refreshToken: String, val accessTokenHetHan: String)

class PhienDataStore(context: Context) {
    private val dataStore = PreferenceDataStoreFactory.create(
        produceFile = { File(context.noBackupFilesDir, "remarket_session.preferences_pb") }
    )
    private val keyName = stringPreferencesKey("phien_ma_hoa_v1")
    private val gson = Gson()
    private val mutation = Mutex()
    val hienTai = AtomicReference<PhienLuu?>(null)

    private fun key(): SecretKey {
        val store = KeyStore.getInstance("AndroidKeyStore").apply { load(null) }
        (store.getKey("remarket_session_v1", null) as? SecretKey)?.let { return it }
        return KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore").apply {
            init(KeyGenParameterSpec.Builder("remarket_session_v1", KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setKeySize(256).build())
        }.generateKey()
    }
    suspend fun doc(): PhienLuu? {
        val value = dataStore.data.first()[keyName] ?: return null.also { hienTai.set(null) }
        return try {
            val bytes = Base64.decode(value, Base64.NO_WRAP)
            require(bytes.size > 28)
            val cipher = Cipher.getInstance("AES/GCM/NoPadding").apply {
                init(Cipher.DECRYPT_MODE, key(), GCMParameterSpec(128, bytes.copyOfRange(0, 12)))
            }
            gson.fromJson(String(cipher.doFinal(bytes.copyOfRange(12, bytes.size)), Charsets.UTF_8), PhienLuu::class.java)
                .also { hienTai.set(it) }
        } catch (_: GeneralSecurityException) { xoa(); null }
          catch (_: IllegalArgumentException) { xoa(); null }
          catch (_: JsonParseException) { xoa(); null }
    }
    suspend fun luu(dto: PhienDto, kyVongRefresh: String? = null): Boolean = mutation.withLock {
        if (kyVongRefresh != null && hienTai.get()?.refreshToken != kyVongRefresh) return@withLock false
        val session = PhienLuu(dto.accessToken, dto.refreshToken, dto.accessTokenHetHan)
        val cipher = Cipher.getInstance("AES/GCM/NoPadding").apply { init(Cipher.ENCRYPT_MODE, key()) }
        val value = Base64.encodeToString(cipher.iv + cipher.doFinal(gson.toJson(session).toByteArray(Charsets.UTF_8)), Base64.NO_WRAP)
        dataStore.edit { it[keyName] = value }
        hienTai.set(session)
        true
    }
    suspend fun xoa(kyVongRefresh: String? = null) = mutation.withLock {
        if (kyVongRefresh != null && hienTai.get()?.refreshToken != kyVongRefresh) return@withLock
        dataStore.edit { it.remove(keyName) }; hienTai.set(null)
    }
}
