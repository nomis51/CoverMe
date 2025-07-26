package com.jetbrains.rider.plugins.coverme.helpers

import com.intellij.util.io.createDirectories
import java.io.IOException
import java.nio.file.*
import java.nio.file.attribute.BasicFileAttributes
import kotlin.io.path.exists

class FileHelper {
    companion object {
        fun copyFolderRecursively(
            sourcePath: Path,
            destinationPath: Path
        ) {
            destinationPath.createDirectories()

            Files.walkFileTree(
                sourcePath,
                object : SimpleFileVisitor<Path>() {
                    override fun preVisitDirectory(
                        dir: Path?,
                        attrs: BasicFileAttributes
                    ): FileVisitResult {
                        if (dir == null) return FileVisitResult.CONTINUE

                        val targetDir = destinationPath.resolve(sourcePath.relativize(dir))
                        try {
                            targetDir.createDirectories()
                        } catch (e: Exception) {
                            return FileVisitResult.SKIP_SUBTREE
                        }
                        return FileVisitResult.CONTINUE
                    }

                    override fun visitFile(
                        file: Path?,
                        attrs: BasicFileAttributes
                    ): FileVisitResult {
                        if (file == null) return FileVisitResult.CONTINUE

                        val targetFile = destinationPath.resolve(sourcePath.relativize(file))
                        try {
                            Files.copy(
                                file,
                                targetFile,
                                StandardCopyOption.REPLACE_EXISTING
                            )
                        } catch (e: Exception) {
                        }
                        return FileVisitResult.CONTINUE
                    }

                    override fun visitFileFailed(
                        file: Path?,
                        exc: IOException
                    ): FileVisitResult {
                        return FileVisitResult.CONTINUE
                    }
                })
        }

        fun deleteFolderRecursively(folderPath: Path) {
            if (!folderPath.exists()) {
                return
            }

            Files.walkFileTree(
                folderPath,
                object : SimpleFileVisitor<Path>() {
                    override fun visitFile(
                        file: Path?,
                        attrs: BasicFileAttributes
                    ): FileVisitResult {
                        if (file == null) return FileVisitResult.CONTINUE

                        try {
                            Files.delete(file)
                        } catch (e: Exception) {
                        }
                        return FileVisitResult.CONTINUE
                    }

                    override fun postVisitDirectory(
                        dir: Path,
                        exc: IOException?
                    ): FileVisitResult {
                        try {
                            Files.delete(dir)
                        } catch (e: Exception) {
                        }
                        return FileVisitResult.CONTINUE
                    }

                    override fun visitFileFailed(
                        file: Path?,
                        exc: IOException
                    ): FileVisitResult {
                        return FileVisitResult.CONTINUE
                    }
                })
        }


    }
}
