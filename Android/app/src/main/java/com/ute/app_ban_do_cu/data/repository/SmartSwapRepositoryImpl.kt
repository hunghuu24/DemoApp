package com.ute.app_ban_do_cu.data.repository
import com.ute.app_ban_do_cu.data.remote.SmartSwapApi
import com.ute.app_ban_do_cu.domain.repository.SmartSwapRepository
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import okhttp3.MultipartBody

class SmartSwapRepositoryImpl(private val api:SmartSwapApi):SmartSwapRepository by api {
    override suspend fun evidence(id:String,key:String,bytes:ByteArray)=api.evidenceForm(id,key.toRequestBody("text/plain".toMediaType()),MultipartBody.Part.createFormData("anh","evidence.png",bytes.toRequestBody("image/png".toMediaType())))
}
