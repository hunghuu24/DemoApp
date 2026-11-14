package com.ute.app_ban_do_cu

import android.os.SystemClock
import okhttp3.Call
import okhttp3.OkHttpClient
import okhttp3.Request
import java.util.ArrayDeque

/** Test fixtures respect the normal server limits; production HTTP clients are unchanged. */
internal object UiTestPacing {
    private val gate = Any()
    private val requests = mutableMapOf<String, ArrayDeque<Long>>()
    private fun waitFor(policy: String, permits: Int, window: Long = 61_000) {
        while (true) {
            val delay = synchronized(gate) {
                val now = SystemClock.elapsedRealtime()
                val recent = requests.getOrPut(policy) { ArrayDeque() }
                while (recent.isNotEmpty() && recent.first <= now - window) recent.removeFirst()
                if (recent.size < permits) { recent.addLast(now); 0L } else recent.first + window - now
            }
            if (delay <= 0) return
            Thread.sleep(delay.coerceAtMost(1_000))
        }
    }
    fun auth() = waitFor("auth", 10)
    fun await(request: Request) {
        val path = request.url.encodedPath
        when {
            path == "/api/smart-swaps/search" || path == "/api/smart-swaps/preview" -> waitFor("search", 4)
            request.method == "GET" || request.method == "HEAD" -> Unit
            path == "/api/taikhoan/dangxuat" -> Unit
            path == "/api/taikhoan/quenmatkhau" -> waitFor("recovery", 3, 901_000)
            path.startsWith("/api/taikhoan/") -> auth()
            path.startsWith("/api/tindang") -> waitFor("listing", 10)
            else -> waitFor("commerce", 35)
        }
    }
}

/** Wait before creating the call, so a fixture delay does not consume its network timeout. */
internal fun OkHttpClient.newUiTestCall(request: Request): Call {
    UiTestPacing.await(request)
    return newCall(request)
}
