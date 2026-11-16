import java.net.URI

plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.compose)
}

val reMarketApiBaseUrl = providers.gradleProperty("REMARKET_API_BASE_URL").orElse("http://10.0.2.2:5080/").get()
val autoStartLocalApi = providers.gradleProperty("REMARKET_AUTO_START_LOCAL").map(String::toBoolean).orElse(false)
val apiAddress = URI(reMarketApiBaseUrl)
val supportsLocalApi = System.getProperty("os.name").startsWith("Windows") &&
    apiAddress.scheme == "http" && apiAddress.host in setOf("127.0.0.1", "localhost", "10.0.2.2") && apiAddress.port == 5080
val localApiScript = rootProject.layout.projectDirectory.file("../tools/Start-ReMarketLocal.ps1").asFile.absolutePath
val prepareReMarketLocalApi by tasks.registering(Exec::class) {
    group = "remarket"
    description = "Start the local API and reconnect authorized ADB devices before a Debug run."
    val enabledOnThisHost = autoStartLocalApi.get() && supportsLocalApi
    onlyIf { enabledOnThisHost }
    commandLine("powershell.exe", "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", localApiScript, "-AutoConnect")
    // No outputs: recheck API/ADB even when the APK itself is up to date.
}
tasks.matching { it.name == "preDebugBuild" }.configureEach { dependsOn(prepareReMarketLocalApi) }

android {
    namespace = "com.ute.app_ban_do_cu"
    compileSdk {
        version = release(37)
    }

    defaultConfig {
        applicationId = "com.ute.app_ban_do_cu"
        minSdk = 24
        targetSdk = 37
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
        val apiBaseUrl = reMarketApiBaseUrl
        require(apiBaseUrl.matches(Regex("https?://[A-Za-z0-9.\\-:/]+/"))) { "API URL must end with / and contain no credentials/query" }
        buildConfigField("String", "API_BASE_URL", "\"$apiBaseUrl\"")
    }

    buildTypes {
        release {
            optimization {
                enable = false
            }
        }
    }
    compileOptions {
        isCoreLibraryDesugaringEnabled = true
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    buildFeatures {
        compose = true
        buildConfig = true
    }
}

dependencies {
    implementation(libs.androidx.core.ktx)
    implementation(platform(libs.compose.bom))
    implementation(libs.compose.ui)
    implementation(libs.compose.material3)
    implementation(libs.compose.preview)
    debugImplementation(libs.compose.tooling)
    implementation(libs.activity.compose)
    implementation(libs.lifecycle.viewmodel.compose)
    implementation(libs.lifecycle.runtime.compose)
    implementation(libs.retrofit)
    implementation(libs.retrofit.gson)
    implementation(libs.datastore.preferences)
    implementation(libs.coil.compose)
    coreLibraryDesugaring(libs.desugar)
    androidTestImplementation(platform(libs.compose.bom))
    androidTestImplementation(libs.compose.test)
    debugImplementation(libs.compose.test.manifest)
    testImplementation(libs.junit)
    androidTestImplementation(libs.androidx.espresso.core)
    androidTestImplementation(libs.androidx.junit)
}
