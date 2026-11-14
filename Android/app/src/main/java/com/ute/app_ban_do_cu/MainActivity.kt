package com.ute.app_ban_do_cu

import android.os.Bundle
import android.content.Intent
import android.net.Uri
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.lifecycle.viewmodel.compose.viewModel
import com.ute.app_ban_do_cu.core.designsystem.ReMarketTheme
import androidx.compose.runtime.mutableStateOf
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanApp
import com.ute.app_ban_do_cu.feature.auth.TaiKhoanViewModel

class MainActivity : ComponentActivity() {
    private val recoveryLink = mutableStateOf<Uri?>(null)
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        recoveryLink.value = intent?.data
        val container = (application as ReMarketApplication).container
        setContent {
            ReMarketTheme {
                val model: TaiKhoanViewModel = viewModel(factory = TaiKhoanViewModel.factory(container.taiKhoanRepository))
                val listings: com.ute.app_ban_do_cu.feature.catalog.BanHangViewModel = viewModel(factory = com.ute.app_ban_do_cu.feature.catalog.BanHangViewModel.factory(container.tinDangRepository))
                val commerce: com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel = viewModel(factory = com.ute.app_ban_do_cu.feature.commerce.MuaBanViewModel.factory(container.muaBanRepository))
                val exchange: com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel = viewModel(factory = com.ute.app_ban_do_cu.feature.exchange.TraoDoiViewModel.factory(container.traoDoiRepository,container.tinDangRepository))
                val smart:com.ute.app_ban_do_cu.feature.smartswap.SmartSwapViewModel=viewModel(factory=com.ute.app_ban_do_cu.feature.smartswap.SmartSwapViewModel.factory(container.smartSwapRepository,container.tinDangRepository,container.muaBanRepository))
                val admin:com.ute.app_ban_do_cu.feature.admin.QuanTriViewModel=viewModel(factory=com.ute.app_ban_do_cu.feature.admin.QuanTriViewModel.factory(container.quanTriRepository))
                TaiKhoanApp(model, container.imageLoader, recoveryLink.value, listings, commerce, exchange,smart,admin)
            }
        }
    }
    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
        recoveryLink.value = intent.data
    }
}
