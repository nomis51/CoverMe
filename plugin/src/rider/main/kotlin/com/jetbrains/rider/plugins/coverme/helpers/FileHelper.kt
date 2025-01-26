package com.jetbrains.rider.plugins.coverme.helpers

import com.intellij.util.io.createDirectories
import com.jetbrains.rider.plugins.coverme.services.LoggingService
import java.io.File
import java.io.IOException
import java.io.RandomAccessFile
import java.nio.channels.FileLock
import java.nio.file.*
import java.nio.file.attribute.BasicFileAttributes
import java.security.MessageDigest
import kotlin.io.path.exists

class FileHelper {
    companion object {
        fun calculateFileSHA(file: File, algorithm: String = "SHA-256"): String {
            if(!file.exists()) return ""

            val buffer = ByteArray(128 * 1024)
            val digest = MessageDigest.getInstance(algorithm)

            file.inputStream().use { inputStream ->
                var bytesRead: Int
                while (inputStream.read(buffer).also { bytesRead = it } != -1) {
                    digest.update(buffer, 0, bytesRead)
                }
            }

            return digest.digest().joinToString("") { "%02x".format(it) }
        }

        fun lockFileAndPerformOperation(filePath: String, operation: () -> Boolean, retryCount: Int = 3): Boolean {
            try {
                val f = File(filePath)
                if (!f.exists()) {
                    f.createNewFile()
                }

                val file = RandomAccessFile(filePath, "rw")
                val fileChannel = file.channel
                var lock: FileLock? = null

                try {
                    lock = fileChannel.lock()
                    LoggingService.getInstance()
                        .info("File lock acquired")

                    return operation()
                } catch (e: Exception) {
                    LoggingService.getInstance()
                        .error("Failed to acquire file lock: ${e.message}")

                    if (retryCount == 0) {
                        LoggingService.getInstance()
                            .warn("Failed to acquire file lock 3 times, giving up")
                        return false
                    }

                    Thread.sleep(3000)
                    return lockFileAndPerformOperation(filePath, operation, retryCount - 1)
                } finally {
                    lock?.release()
                    fileChannel.close()
                    file.close()
                    LoggingService.getInstance()
                        .info("File lock released")
                }
            } catch (e: Exception) {
                LoggingService.getInstance()
                    .error("Failed to access lock file: ${e.message}")
            }

            return false
        }

        fun copyFolderRecursively(sourcePath: Path, destinationPath: Path) {
            sourcePath.createDirectories()

            Files.walkFileTree(sourcePath, object : SimpleFileVisitor<Path>() {
                override fun preVisitDirectory(dir: Path?, attrs: BasicFileAttributes): FileVisitResult {
                    if (dir == null) return FileVisitResult.CONTINUE

                    val targetDir = destinationPath.resolve(sourcePath.relativize(dir))
                    try {
                        targetDir.createDirectories()
                    } catch (e: Exception) {
                        LoggingService.getInstance()
                            .error("Failed to create directory: $targetDir. Error: ${e.message}")
                        return FileVisitResult.SKIP_SUBTREE
                    }
                    return FileVisitResult.CONTINUE
                }

                override fun visitFile(file: Path?, attrs: BasicFileAttributes): FileVisitResult {
                    if (file == null) return FileVisitResult.CONTINUE

                    val targetFile = destinationPath.resolve(sourcePath.relativize(file))
                    try {
                        Files.copy(file, targetFile, StandardCopyOption.REPLACE_EXISTING)
                    } catch (e: Exception) {
                        LoggingService.getInstance()
                            .error("Failed to copy file: $file to $targetFile. Error: ${e.message}")
                    }
                    return FileVisitResult.CONTINUE
                }

                override fun visitFileFailed(file: Path?, exc: IOException): FileVisitResult {
                    LoggingService.getInstance()
                        .error("Failed to visit file: $file. Error: ${exc.message}")
                    return FileVisitResult.CONTINUE
                }
            })
        }

        fun deleteFolderRecursively(folderPath: Path) {
            if (!folderPath.exists()) {
                LoggingService.getInstance()
                    .warn("Delete folder recursively: folder does not exist: $folderPath")
                return
            }

            Files.walkFileTree(folderPath, object : SimpleFileVisitor<Path>() {
                override fun visitFile(file: Path?, attrs: BasicFileAttributes): FileVisitResult {
                    if (file == null) return FileVisitResult.CONTINUE

                    try {
                        Files.delete(file)
                    } catch (e: Exception) {
                        LoggingService.getInstance()
                            .warn("Failed to delete file: $file. Error: ${e.message}")
                    }
                    return FileVisitResult.CONTINUE
                }

                override fun postVisitDirectory(dir: Path, exc: IOException?): FileVisitResult {
                    try {
                        Files.delete(dir)
                    } catch (e: Exception) {
                        LoggingService.getInstance()
                            .warn("Failed to delete directory: $dir. Error: ${e.message}")
                    }
                    return FileVisitResult.CONTINUE
                }

                override fun visitFileFailed(file: Path?, exc: IOException): FileVisitResult {
                    LoggingService.getInstance()
                        .warn("Failed to visit file: $file. Error: ${exc.message}")
                    return FileVisitResult.CONTINUE
                }
            })
        }
    }
}