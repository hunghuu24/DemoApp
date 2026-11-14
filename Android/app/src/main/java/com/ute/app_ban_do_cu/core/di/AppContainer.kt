package com.ute.app_ban_do_cu.core.di

import android.content.Context
import coil.ImageLoader
import com.ute.app_ban_do_cu.BuildConfig
import com.ute.app_ban_do_cu.data.remote.ReMarketApi
import com.ute.app_ban_do_cu.data.repository.HeThongRepositoryImpl
import com.ute.app_ban_do_cu.domain.repository.HeThongRepository
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit
import com.ute.app_ban_do_cu.data.local.PhienDataStore
import com.ute.app_ban_do_cu.data.remote.TaiKhoanApi
import com.ute.app_ban_do_cu.data.remote.TokenInterceptor
import com.ute.app_ban_do_cu.data.remote.PhienAuthenticator
import com.ute.app_ban_do_cu.data.repository.TaiKhoanRepositoryImpl
import com.ute.app_ban_do_cu.domain.repository.TaiKhoanRepository

class AppContainer(context: Context) {
    private val client = OkHttpClient.Builder().connectTimeout(5, TimeUnit.SECONDS).readTimeout(10, TimeUnit.SECONDS).build()
    private val api = Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(client)
        .addConverterFactory(GsonConverterFactory.create()).build().create(ReMarketApi::class.java)
    val heThongRepository: HeThongRepository = HeThongRepositoryImpl(api)
    val phienStore = PhienDataStore(context)
    private fun accountApi(client: OkHttpClient) = Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL)
        .client(client).addConverterFactory(GsonConverterFactory.create()).build().create(TaiKhoanApi::class.java)
    private val publicApi = accountApi(client)
    // A separate dispatcher keeps refresh requests from waiting behind blocked authenticated requests.
    private val authenticatedClient = OkHttpClient.Builder().connectTimeout(10, TimeUnit.SECONDS)
        .readTimeout(30, TimeUnit.SECONDS).addInterceptor(TokenInterceptor(phienStore))
        .authenticator(PhienAuthenticator(phienStore, publicApi)).build()
    val taiKhoanRepository: TaiKhoanRepository = TaiKhoanRepositoryImpl(publicApi, accountApi(authenticatedClient), phienStore)
    val imageLoader: ImageLoader = ImageLoader.Builder(context).okHttpClient(authenticatedClient).build()
    private val listingApi = Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(authenticatedClient)
        .addConverterFactory(GsonConverterFactory.create()).build().create(com.ute.app_ban_do_cu.data.remote.TinDangApi::class.java)
    val tinDangRepository: com.ute.app_ban_do_cu.domain.repository.TinDangRepository =
        com.ute.app_ban_do_cu.data.repository.TinDangRepositoryImpl(listingApi)
    private val commerceApi = Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(authenticatedClient)
        .addConverterFactory(GsonConverterFactory.create()).build().create(com.ute.app_ban_do_cu.data.remote.MuaBanApi::class.java)
    val muaBanRepository: com.ute.app_ban_do_cu.domain.repository.MuaBanRepository =
        com.ute.app_ban_do_cu.data.repository.MuaBanRepositoryImpl(commerceApi)
    private val exchangeApi=Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(authenticatedClient)
        .addConverterFactory(GsonConverterFactory.create()).build().create(com.ute.app_ban_do_cu.data.remote.TraoDoiApi::class.java)
    val traoDoiRepository:com.ute.app_ban_do_cu.domain.repository.TraoDoiRepository=
        com.ute.app_ban_do_cu.data.repository.TraoDoiRepositoryImpl(exchangeApi,listingApi)
    private val smartApi=Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(authenticatedClient)
        .addConverterFactory(GsonConverterFactory.create()).build().create(com.ute.app_ban_do_cu.data.remote.SmartSwapApi::class.java)
    val smartSwapRepository:com.ute.app_ban_do_cu.domain.repository.SmartSwapRepository=
        com.ute.app_ban_do_cu.data.repository.SmartSwapRepositoryImpl(smartApi)
    private val adminApi=Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL).client(authenticatedClient)
        .addConverterFactory(GsonConverterFactory.create()).build().create(com.ute.app_ban_do_cu.data.remote.QuanTriApi::class.java)
    val quanTriRepository:com.ute.app_ban_do_cu.domain.repository.QuanTriRepository=com.ute.app_ban_do_cu.data.repository.QuanTriRepositoryImpl(adminApi)
}
