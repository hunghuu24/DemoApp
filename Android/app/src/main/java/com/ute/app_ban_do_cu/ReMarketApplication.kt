package com.ute.app_ban_do_cu

import android.app.Application
import com.ute.app_ban_do_cu.core.di.AppContainer

class ReMarketApplication : Application() {
    val container: AppContainer by lazy { AppContainer(this) }
}
