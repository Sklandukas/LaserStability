namespace LaserStability.Helpers
{
    public static class ImageLoader
    {
        public static List<string> GetImagePaths(string folderPath)
        {
            var extensions = new[] { ".bmp" };
            return Directory.GetFiles(folderPath)
                            .Where(file => extensions.Contains(Path.GetExtension(file).ToLower()))
                            .ToList();
        }
    }
}