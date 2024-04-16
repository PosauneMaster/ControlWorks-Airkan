namespace ControlWorks.Services.PVI.Models
{
    public class AirkanInputFileInfo
    {
        private readonly bool _fileTransferLocation;
        public AirkanInputFileInfo()
        {
        }

        public AirkanInputFileInfo(int index, bool fileTransferLocation, string path)
        {
            Index = index;
            _fileTransferLocation = fileTransferLocation;
            Path = path;
        }

        public int Index { get; set; }

        public string FileTransferLocation => _fileTransferLocation ? $"True (1) USB" : $"False (0) Network Drive";
        public string Path { get; set; }
    }
}
