using System;
using System.Collections.Generic;
using System.Text;

namespace Khonshu.Khonshu.Prikoli
{
    public sealed class DiskInfo
    {
        public List<String> DiskNames { get; private set; } = new List<String>();
        public List<String> DiskType {  get; private set; } = new List<String>();
        public List<String> DiskFormat { get; private set; } = new List<String>();
        public List<String> DiskTotalSize { get; private set; } = new List<String>();
        public List<String> DiskFreeSpace { get; private set; } = new List<String>();
        public DiskInfo()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives() ) 
            {
                if(drive.IsReady)
                {
                    DiskNames.Add(drive.Name);
                    DiskType.Add(drive.DriveType.ToString());
                    DiskFormat.Add(drive.DriveFormat);
                    DiskTotalSize.Add((drive.TotalSize / (1024 * 1024 * 1024)).ToString() + " GB");
                    DiskFreeSpace.Add((drive.TotalFreeSpace / (1024 * 1024 * 1024)).ToString() + " GB");
                }
            }
        }

        public List<List<String>> GetDiskFullInfo()
        {
            List<List<String>> fullInfo = new List<List<String>>();
            fullInfo.Add(DiskNames);
            fullInfo.Add(DiskType);
            fullInfo.Add(DiskFormat);
            fullInfo.Add(DiskTotalSize);
            fullInfo.Add(DiskFreeSpace);
            return fullInfo;
        }
    }
}
