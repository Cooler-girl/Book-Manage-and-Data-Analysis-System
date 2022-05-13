namespace BookMS.Help
{
    public static class FileHelper
    {
        #region 拷贝文件
        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="orignFile">源文件路径</param>
        /// <param name="newFile">目标文件的路径</param>
        /// <exception cref="ArgumentException"></exception>
        public static void FileCoppy(string orignFile, string newFile)
        {
            if (string.IsNullOrEmpty(orignFile))
            {
                throw new ArgumentException(orignFile);
            }
            if (string.IsNullOrEmpty(newFile))
            {
                throw new ArgumentException(newFile);
            }
            System.IO.File.Copy(orignFile, newFile, true);
        }
        #endregion

        #region 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">要删除的文件的路径</param>
        /// <exception cref="ArgumentException"></exception>
        public static void FileDel(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(path);
            }
            System.IO.File.Delete(path);
        }
        #endregion

        #region 移动文件
        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="orignFile">原始路径</param>
        /// <param name="newFile">新路径</param>
        /// <exception cref="ArgumentException"></exception>
        public static void FileMove(string orignFile, string newFile)
        {
            if (string.IsNullOrEmpty(orignFile))
            {
                throw new ArgumentException(orignFile);
            }
            if (string.IsNullOrEmpty(newFile))
            {
                throw new ArgumentException(newFile);
            }
            System.IO.File.Move(orignFile, newFile);
        }
        #endregion

        #region 创建路径（文件夹）
        /// <summary>
        /// 创建路径
        /// </summary>
        /// <param name="FilePath">路径</param>
        public static void CreatePath(string FilePath)
        {
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
        }
        #endregion

        #region 创建文件
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="FilePath"></param>
        public static void CreateFile(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                FileStream fs = File.Create(FilePath);
                fs.Close();
            }
        }
        #endregion

    }
}
