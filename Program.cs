using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;
using Serilog;
using SmartFormat;

namespace MediaOrganizer
{
    internal class Program
    {
        // TODO: Make paths configurable
        // TODO: Provide option to move instead of copy
        // TODO: Use checksums for integrity
        // TODO: Provide more string params

        private const string InputMediaDir = "C:/Users/jean.farrugia/OneDrive/Media";
        private const string OutputMediaDir = "C:/Organized/{Year}/{Month}/{Day}";
        private const string OutputUnknownDateMediaDir = "C:/Organized/UnknownDate";
        private const string OutputDuplicateMediaDir = "C:/Organized/Duplicates";
        private const string OutputNotSupportedDir = "C:/Organized/NotSupported";

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            new MediaOrganizer(
                    inputMediaDir: InputMediaDir,
                    outputMediaDir: OutputMediaDir,
                    outputUnknownDateMediaDir: OutputUnknownDateMediaDir,
                    outputDuplicateMediaDir: OutputDuplicateMediaDir,
                    outputNotSupportedDir: OutputNotSupportedDir
                ).Organize();

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}