package com.ute.app_ban_do_cu.data.remote

import com.ute.app_ban_do_cu.data.local.PhienDataStore
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import okhttp3.Authenticator
import okhttp3.Interceptor
import okhttp3.Request
import okhttp3.Response
import okhttp3.Route
import retrofit2.HttpException
import java.io.IOException
import com.ute.app_ban_do_cu.domain.model.RefreshBody
import com.ute.app_ban_do_cu.BuildConfig
import okhttp3.HttpUrl.Companion.toHttpUrl

class TokenInterceptor(private val store: PhienDataStore) : Interceptor {
    override fun intercept(chain: Interceptor.Chain): Response {
        val allowed = BuildConfig.API_BASE_URL.toHttpUrl()
        val url = chain.request().url
        if (url.host != allowed.host || url.port != allowed.port || url.scheme != allowed.scheme)
            return chain.proceed(chain.request())
        val token = store.hienTai.get()?.accessToken
        val request = chain.request().newBuilder().apply {
            if (token != null) header("Authorization", "Bearer $token")
        }.build()
        return chain.proceed(request)
    }
}

class PhienAuthenticator(private val store: PhienDataStore, private val publicApi: TaiKhoanApi) : Authenticator {
    private val mutex = Mutex()
    override fun authenticate(route: Route?, response: Response): Request? {
        if (response.priorResponse != null || response.request.header("Authorization") == null) return null
        return runBlocking {
            mutex.withLock {
                val current = store.hienTai.get() ?: return@withLock null
                val previous = response.request.header("Authorization")
                if (previous != "Bearer ${current.accessToken}")
                    return@withLock response.request.newBuilder().header("Authorization", "Bearer ${current.accessToken}").build()
                try {
                    val next = publicApi.lamMoi(RefreshBody(current.refreshToken))
                    if (!store.luu(next, current.refreshToken)) return@withLock null
                    response.request.newBuilder().header("Authorization", "Bearer ${next.accessToken}").build()
                } catch (ex: HttpException) {
                    if (ex.code() == 401 || ex.code() == 403) store.xoa(current.refreshToken)
                    null
                } catch (_: IOException) { null }
            }
        }
    }
}
