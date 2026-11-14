package com.ute.app_ban_do_cu.feature.account

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.ute.app_ban_do_cu.R
import com.ute.app_ban_do_cu.core.designsystem.*
import com.ute.app_ban_do_cu.feature.auth.*
import java.text.NumberFormat
import java.util.Locale

@Composable
fun TrangChuScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel) {
    Column(Modifier.fillMaxSize().testTag("user_home").verticalScroll(rememberScrollState()).padding(horizontal = 24.dp, vertical = 12.dp),
        verticalArrangement = Arrangement.spacedBy(20.dp)) {
        Text("Đồ cũ, hành trình mới", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
        RmCard {
            Row(horizontalArrangement = Arrangement.spacedBy(10.dp), verticalAlignment = Alignment.CenterVertically) {
                RmIcon(R.drawable.ic_rm_search, size = 18.dp)
                Text("Tìm món đồ bạn cần", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
            }
        }
        Text("Khám phá danh mục", style = MaterialTheme.typography.titleMedium)
        val categories = state.trangChu?.danhMuc.orEmpty()
        categories.chunked(3).forEach { row ->
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                row.forEach { category ->
                    Box(Modifier.weight(1f).heightIn(min = 44.dp).background(ReMarketPalette.Soft, RoundedCornerShape(12.dp)).padding(8.dp), contentAlignment = Alignment.Center) {
                        Text(category.ten, style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Primary)
                    }
                }
            }
        }
        Text("Sản phẩm nổi bật", style = MaterialTheme.typography.titleMedium)
        state.trangChu?.sanPham?.forEach { product ->
            RmCard {
                Text(product.ten, style = MaterialTheme.typography.titleSmall)
                Spacer(Modifier.height(8.dp))
                Text(NumberFormat.getIntegerInstance(Locale.forLanguageTag("vi-VN")).format(product.giaBan ?: 0) + " đ",
                    style = rmText(17, FontWeight.Bold), color = ReMarketPalette.Primary)
                Spacer(Modifier.height(6.dp))
                Text(product.tinhThanh, style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted)
            }
        }
        if (!state.dangTai && state.trangChu?.sanPham?.isEmpty() == true) RmNotice("Chưa có sản phẩm đang bán. Hãy quay lại sau nhé.")
        if (state.loi != null) RmButton("Thử tải lại", { vm.taiTrangChinh() }, outlined = true)
    }
}

@Composable
fun QuanTriScreen(state: TaiKhoanUiState, vm: TaiKhoanViewModel, review: () -> Unit = {}, support: () -> Unit = {}) {
    val stats = state.tongQuan
    Column(Modifier.fillMaxSize().testTag("admin_home").verticalScroll(rememberScrollState()).padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
        Text("Chào ${state.hoSo?.hoTen.orEmpty()}", style = rmText(19, FontWeight.Bold))
        Text("Tổng quan hoạt động cộng đồng", style = MaterialTheme.typography.bodyLarge, color = ReMarketPalette.Muted)
        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            Kpi("Tổng người dùng", stats?.tongNguoiDung?.toString() ?: "—", Modifier.weight(1f))
            Kpi("Tin chờ duyệt", stats?.tinChoDuyet?.toString() ?: "—", Modifier.weight(1f))
        }
        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            Kpi("Mua bán / Trao đổi", if (stats == null) "—" else "${stats.muaBan} / ${stats.traoDoi}", Modifier.weight(1f))
            Kpi("Tranh chấp cần xử lý", stats?.tranhChap?.toString() ?: "—", Modifier.weight(1f))
        }
        RmCard {
            Text("Lượng giao dịch", style = MaterialTheme.typography.titleMedium)
            if (stats != null) {
                Spacer(Modifier.height(20.dp))
                val largest = maxOf(stats.muaBan, stats.traoDoi, 1)
                listOf("Mua bán" to stats.muaBan, "Trao đổi" to stats.traoDoi).forEach { (label, value) ->
                    Text("$label: $value", style = MaterialTheme.typography.bodySmall)
                    Box(Modifier.padding(vertical = 8.dp).fillMaxWidth().height(14.dp).background(ReMarketPalette.Soft, RoundedCornerShape(4.dp))) {
                        Box(Modifier.fillMaxHeight().fillMaxWidth(value.toFloat() / largest).background(ReMarketPalette.Primary, RoundedCornerShape(4.dp)))
                    }
                }
            }
        }
        RmCard {
            Text("Việc cần làm", style = MaterialTheme.typography.titleMedium)
            Spacer(Modifier.height(12.dp))
            Text("${stats?.tinChoDuyet ?: 0} tin đang chờ kiểm duyệt", style = MaterialTheme.typography.bodyLarge)
        }
        RmButton("Kiểm duyệt tin đăng", review, Modifier.testTag("admin_review"), outlined = true)
        RmButton("Quản lý yêu cầu hỗ trợ", support, Modifier.testTag("admin_support"), outlined = true)
        if (state.loi != null) RmButton("Thử tải lại", { vm.taiTrangChinh() }, outlined = true)
    }
}

@Composable
private fun Kpi(label: String, value: String, modifier: Modifier) {
    RmCard(modifier) {
        Text(label, style = MaterialTheme.typography.bodySmall, color = ReMarketPalette.Muted)
        Spacer(Modifier.height(12.dp))
        Text(value, style = rmText(25, FontWeight.Bold), color = ReMarketPalette.Primary)
    }
}
