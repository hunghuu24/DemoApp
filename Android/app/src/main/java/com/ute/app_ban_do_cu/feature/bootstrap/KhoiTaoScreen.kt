package com.ute.app_ban_do_cu.feature.bootstrap

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.compose.collectAsStateWithLifecycle

@Composable
fun KhoiTaoScreen(viewModel: KhoiTaoViewModel) {
    val state by viewModel.uiState.collectAsStateWithLifecycle()
    Scaffold { insets ->
        Column(Modifier.fillMaxSize().padding(insets).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
            Text("ReMarket", style = MaterialTheme.typography.headlineLarge)
            Text("Đồ cũ, hành trình mới")
            Text(state.thongBao)
            Button(onClick = viewModel::kiemTraKetNoi, enabled = !state.dangTai) {
                Text(if (state.dangTai) "Đang kiểm tra…" else "Kiểm tra kết nối")
            }
        }
    }
}
