using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Serilog;
using SmartFormat;

namespace MediaOrganizer
{
    internal class MediaOrganizer
    {
        public MediaOrganizer(
                string inputMediaDir,
                string outputMediaDir,
                string outputUnknownDateMediaDir,
                string outputDuplicateMediaDir,
                string outputNotSupportedDir
            )
        {
            InputMediaDir = inputMediaDir;
            OutputMediaDir = outputMediaDir;
            OutputUnknownDateMediaDir = outputUnknownDateMediaDir;
            OutputDuplicateMediaDir = outputDuplicateMediaDir;
            OutputNotSupportedDir = outputNotSupportedDir;
        }

        private static readonly IList<FileAssociation> FileNameAssociations = new List<FileAssociation>
        {
            new FileAssociation
            {
                //IMG_120624372003024.jpg
                RegexMatch = new Regex("^IMG_[0-9]{14,16}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //VID_20170628_230305.mp4
                //VID_20170628_230305~1.mp4
                //PANO_20150719_160215.jpg
                //IMG_20170630_140333_01.jpg
                //IMG_20170630_140333~01.jpg
                //IMG_20170630_140333 01.jpg
                RegexMatch = new Regex("^(VID|PANO|IMG)_[0-9]{8,8}_[0-9]{6,6}((_|~|\\s)[0-9]{1,3})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //20170628_145804000_iOS.MOV
                RegexMatch = new Regex("^[0-9]{8,8}_[0-9]{9,9}_iOS\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //2013-09-23 21.17.47.jpg
                //2013-09-27 11.57.08-1.jpg
                RegexMatch = new Regex("^[0-9]{4,4}-[0-9]{2,2}-[0-9]{2,2}\\s[0-9]{2,2}.[0-9]{2,2}.[0-9]{2,2}(-[0-9]{1,2})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //IMAG0089.jpg
                //VIDEO0010.mp4
                //DSCN3376.JPG
                RegexMatch = new Regex("^(IMAG|VIDEO|DSCN)[0-9]{4,4}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //168A3663.jpg
                RegexMatch = new Regex("^168A[0-9]{4,4}\\.[a-zA-Z0-9]{2,3}$", RegexOptions.IgnoreCase),
                Tag = "Camera"
            },
            new FileAssociation
            {
                //received_10213452414052570.jpg
                //received_10204931109916782 1.jpeg
                //received_10154410865625878~2.png
                RegexMatch = new Regex("^received_(-)?[0-9]{9,17}((\\s|~)[0-9]{1,2})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Messenger"
            },
            new FileAssociation
            {
                //1497981783009.jpg
                RegexMatch = new Regex("^[0-9]{13,13}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Messenger"
            },
            new FileAssociation
            {
                //FB_IMG_1497608469236.jpg
                RegexMatch = new Regex("^FB_IMG_[0-9]{13,13}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Facebook"
            },
            new FileAssociation
            {
                //IMG-20170628-WA0003.jpeg
                RegexMatch = new Regex("^IMG-[0-9]{8,8}-WA[0-9]{4,4}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Whatsapp"
            },
            new FileAssociation
            {
                //jeaa024_1497370194791.jpg
                RegexMatch = new Regex("^[a-zA-Z0-9\\.]*_[0-9]{13,13}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Snapchat"
            },
            new FileAssociation
            {
                //Snapchat-2150957531342551500.jpg
                RegexMatch = new Regex("Snapchat-[0-9]{9,19}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Snapchat"
            },
            new FileAssociation
            {
                //sterling-ryan_2014-04-30_17-17-48.jpg
                RegexMatch = new Regex("^[a-zA-Z0-9-\\.]+_[0-9]{4,4}(-[0-9]{2,2}){2,2}_[0-9]{2,2}-[0-9]{2,2}-[0-9]{2,2}\\.[a-zA-Z0-9]{2,3}$", RegexOptions.IgnoreCase),
                Tag = "Snapchat"
            },
            new FileAssociation
            {
                //19050991_1249312888524710_4594791386611449856_n.jpg
                RegexMatch = new Regex("^.*[0-9]{3,8}_[0-9]{13,17}_[0-9]{8,19}_(n|o)\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Instagram"
            },
            new FileAssociation
            {
                //Screenshot_20170614-035751.jpg
                //Screenshot_20160727-193905~2.png
                RegexMatch = new Regex("^Screenshot_[0-9]{8,8}-[0-9]{6,6}(~[0-9]{1,2})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Screenshots"
            },
            new FileAssociation
            {
                //2017_03_23_22_41_14.mp4
                RegexMatch = new Regex("^[0-9]{4,4}(_[0-9]{2,2}){5,5}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Screenshots"
            },
            new FileAssociation
            {
                //Screenshot 2013-11-12 21.09.30.png
                RegexMatch = new Regex("^Screenshot\\s[0-9]{4,4}(-[0-9]{2,2}){2,2}\\s[0-9]{2,2}\\.[0-9]{2,2}\\.[0-9]{2,2}\\.[a-zA-Z0-9]{2,3}$", RegexOptions.IgnoreCase),
                Tag = "Screenshots"
            },
            new FileAssociation
            {
                //Screenshot_2015-08-27-19-14-18.png
                //Screenshot_2015-05-10-12-56-15~2.jpg
                RegexMatch = new Regex("^Screenshot_[0-9]{4,4}-[0-9]{2,2}-[0-9]{2,2}-[0-9]{2,2}-[0-9]{2,2}-[0-9]{2,2}(~[0-9]{1,2})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "Screenshots"
            },
            new FileAssociation
            {
                //Snapshot_20111231_217.JPG
                RegexMatch = new Regex("^Snapshot_[0-9]{8,8}(_[0-9]{1,3})?\\.[a-zA-Z0-9]{2,3}$", RegexOptions.IgnoreCase),
                Tag = "Webcam"
            },
            new FileAssociation
            {
                //WIN_20141231_234248.MP4
                RegexMatch = new Regex("^WIN_[0-9]{8,8}_[0-9]{6,6}\\.[a-zA-Z0-9]{2,3}$", RegexOptions.IgnoreCase),
                Tag = "Webcam"
            },
            new FileAssociation
            {
                //08 05 2017 9 32 pm Office Lens.jpg
                RegexMatch = new Regex("^([0-9]{1,4}\\s){5,5}(am|pm)\\sOffice\\sLens(\\s[0-9]{1,3})?\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "OfficeLens"
            },
            new FileAssociation
            {
                //Office Lens 20161127-235737.jpg
                RegexMatch = new Regex("^Office\\sLens\\s[0-9]{8,8}-[0-9]{6,8}\\.[a-zA-Z0-9]{2,4}$", RegexOptions.IgnoreCase),
                Tag = "OfficeLens"
            }
        };

        private static readonly ILogger Logger = new LoggerConfiguration()
            .WriteTo.LiterateConsole()
            .WriteTo.File("Logs/log.txt")
            .CreateLogger();

        public string InputMediaDir { get; }
        public string OutputMediaDir { get; }
        public string OutputUnknownDateMediaDir { get; }
        public string OutputDuplicateMediaDir { get; }
        public string OutputNotSupportedDir { get; }

        public void Organize()
        {
            Logger.Information("Starting organization...");

            var files = GetFiles();
            int oldProgress = 0;
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];

                var newProgress = Convert.ToInt32(((float)i / files.Length) * 100);
                if (newProgress != oldProgress)
                {
                    // indicate progress
                    oldProgress = newProgress;
                    Logger.Information($"Progress: {newProgress}% ({i}/{files.Length})");
                }

                try
                {
                    // try get file date
                    var dateTime = GetDate(file);

                    if (dateTime.HasValue)
                    {
                        // fill directory format
                        var newPath = Smart.Format(OutputMediaDir, new
                        {
                            Year = dateTime.Value.Year,
                            Month = dateTime.Value.Month.ToString("00"),
                            Day = dateTime.Value.Day.ToString("00")
                        });

                        // try get file association
                        var association = GetFileAssociation(file);

                        // append path to include tag sub folder
                        if (association != null)
                            newPath = $"{newPath}/{association.Tag}";

                        CopyFile(file, newPath);
                    }
                    else
                    {
                        // failed date extraction
                        CopyFile(file, OutputUnknownDateMediaDir);
                    }
                }
                catch (ImageProcessingException ipe)
                {
                    Logger.Error(ipe, $"ImageProcessingException: {file.Name}");
                    CopyFile(file, OutputNotSupportedDir);
                }
            }

            Logger.Information("Finished organization...");
        }

        private static FileAssociation GetFileAssociation(FileInfo file)
        {
            // try find file association
            FileAssociation foundAssociation = null;
            var last = FileNameAssociations.Last();

            foreach (var assosiation in FileNameAssociations)
            {
                var match = assosiation.RegexMatch.Match(file.Name);

                if (match.Success)
                {
                    if (foundAssociation == null)
                    {
                        foundAssociation = assosiation;
                    }
                    else
                    {
                        Logger.Warning($"File matched multiple file association definitions: {file.FullName} {assosiation.Tag} {assosiation.RegexMatch}");
                    }
                }

                if (foundAssociation == null && assosiation.Equals(last))
                    Logger.Information($"File name could not be categorized: {file.FullName}");
            }

            return foundAssociation;
        }

        private static IReadOnlyList<MetadataExtractor.Directory> GetFileMetaDirectories(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(paramName: nameof(file));

            IReadOnlyList<MetadataExtractor.Directory> directories = null;

            try
            {
                directories = ImageMetadataReader.ReadMetadata(file.FullName);
            }
            catch (IOException ioe)
            {
                // File corruption or unreadable, mostly
                Logger.Error(ioe, $"IOException: {file.FullName}");
            }
            catch (NotSupportedException nse)
            {
                Logger.Error(nse, $"NotSupportedException: {file.FullName}");
            }

            return directories;
        }

        private static DateTime? GetMediaFileMetaDate(IReadOnlyList<MetadataExtractor.Directory> directories, FileInfo file)
        {
            if (directories == null)
                throw new ArgumentNullException(paramName: nameof(directories));

            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

            //foreach (var directory in directories)
            //    foreach (var tag in directory.Tags)
            //        Console.WriteLine($"[{file.Name}] ({directory.GetType()}) {directory.Name} - {tag.Name} = {tag.Description}");

            if (subIfdDirectory != null)
            {
                try
                {
                    var dateTime = DateTime.MinValue;

                    if (dateTime == DateTime.MinValue)
                        subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out dateTime);

                    if (dateTime == DateTime.MinValue)
                        subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTime);

                    if (dateTime == DateTime.MinValue)
                        subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out dateTime);

                    return dateTime == DateTime.MinValue ? null : (DateTime?)dateTime;
                }
                catch (MetadataException me)
                {
                    Logger.Error(me, $"MetadataException: {file.FullName}");
                }
            }

            return null;
        }

        private static DateTime? GetFileMetaDate(IReadOnlyList<MetadataExtractor.Directory> directories)
        {
            if (directories == null)
                throw new ArgumentNullException(paramName: nameof(directories));

            var fileMetadDirectory = directories.OfType<FileMetadataDirectory>().FirstOrDefault();

            var dateTime = DateTime.MinValue;
            fileMetadDirectory?.TryGetDateTime(FileMetadataDirectory.TagFileModifiedDate, out dateTime);

            return dateTime == DateTime.MinValue ? null : (DateTime?)dateTime;
        }

        public static DateTime? GetDate(FileInfo file)
        {
            if (file == null)
                throw new ArgumentNullException(paramName: nameof(file));

            var directories = GetFileMetaDirectories(file);

            if (directories == null)
                return null;

            var dateTime = GetMediaFileMetaDate(directories, file) ?? GetFileMetaDate(directories);

            Logger.Debug($"File: {file.FullName} ({dateTime})");

            return dateTime;
        }

        public void CopyFile(FileInfo file, string newPath)
        {
            System.IO.Directory.CreateDirectory(newPath);
            var fullPath = $"{newPath}/{file.Name}";

            if (File.Exists(fullPath))
            {
                System.IO.Directory.CreateDirectory(OutputDuplicateMediaDir);

                try
                {
                    File.Copy(file.FullName, $"{OutputDuplicateMediaDir}/{file.Name}");
                }
                catch (IOException ioe)
                {
                    Logger.Error(ioe, $"IOException: {file.FullName}");
                }
            }
            else
            {
                try
                {
                    File.Copy(file.FullName, fullPath);
                }
                catch (IOException ioe)
                {
                    Logger.Error(ioe, $"IOException: {file.FullName}");
                }
            }
        }

        public FileInfo[] GetFiles()
        {
            var directory = new DirectoryInfo(InputMediaDir);

            return directory.GetFiles("*.*", SearchOption.AllDirectories);
        }
    }
}