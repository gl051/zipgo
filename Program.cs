using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ionic.Zip;
using System.Text.RegularExpressions;
using System.IO;


namespace zipgo
{
    class Program
    {
        static void Main(string[] args)
        {
            bool bRun = true;
            bool bArgsError = false;
            ZipFile zip;
            String[] filenames;
            // List of files added to the zip archive
            List<string> filesArchived = new List<string>(1000);
                        
            // Parameters default values:
            string ErrorMessage = null;
            string SearchPattern = "*.*";
            string InputDirectory = System.IO.Directory.GetCurrentDirectory();
            string OutputDirectory = System.IO.Directory.GetCurrentDirectory();
            int IntervalMinutes =  5;
            
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string sArg = args[i];
                if (sArg[0].Equals('-'))
                {
                    sArg = sArg.Remove(0, 1);
                    if (sArg.ToLower().Equals("i"))
                    {
                        InputDirectory = args[++i];
                        if (!System.IO.Directory.Exists(InputDirectory))
                        {
                            bArgsError = true;
                            ErrorMessage = "Input directory does not exist";
                        }
                    }
                    else if (sArg.ToLower().Equals("o"))
                    {
                        OutputDirectory = args[++i];
                        if (!System.IO.Directory.Exists(OutputDirectory))
                        {
                            bArgsError = true;
                            ErrorMessage = "Output directory does not exist";
                        }
                    }
                    else if (sArg.ToLower().Equals("p"))
                        SearchPattern = args[++i];
                }
                else
                {
                    try
                    {
                        IntervalMinutes = Int32.Parse(args[i]);
                    }
                    catch
                    {
                        bArgsError = true;
                        ErrorMessage = "Interval must be an integer representing minutes";
                        break;
                    }
                }
            }

            if (bArgsError)
            {
                Console.WriteLine(ErrorMessage);
                Usage();
                return;
            }
            
            try
            {
                while (bRun)
                {

                    filenames = System.IO.Directory.GetFiles(InputDirectory, SearchPattern);
                    if (filenames.Length > 0)
                    {
                        string zipname = OutputDirectory + "\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".zip";
                        zip = new ZipFile(zipname);
                        
                        Console.WriteLine("[{0}] - found {1} files, archive in zip file", DateTime.Now.ToString("HH:mm:ss"), filenames.Length);

                        foreach (String filename in filenames)
                        {
                            if (!IsFileLocked(new FileInfo(filename)))
                            {
                                Console.WriteLine("Add {0}...", filename);
                                ZipEntry e = zip.AddFile(filename);
                                filesArchived.Add(filename);
                            }
                            else
                                Console.WriteLine("File skipped since in use by others: {0}", filename);
                        }

                        if (filesArchived.Count > 0)
                        {
                            zip.Save();
                            Console.WriteLine("Zip file {0} saved", zip.Name);

                            // delete file now that save is ok
                            foreach (String filename in filesArchived)
                            {
                                System.IO.File.Delete(filename);
                            }

                            filesArchived.Clear();
                        }
                        
                        // The frequency is in the order of minutes, dispose the object before and wait for 
                        // the next iteration. Not doing so I was getting an error when looping the second 
                        // time ("file .zip not found")
                        zip.Dispose();
                    }
                    else
                    {
                        Console.WriteLine("[{0}] - Not files found in the directory", DateTime.Now.ToString("HH:mm:ss"));
                    }

                    Console.WriteLine("Sleeping for {0} minutes", IntervalMinutes);
                    Thread.Sleep(IntervalMinutes * 60 * 1000);
                }

            }
            catch (System.Exception ex1)
            {
                System.Console.Error.WriteLine("exception: " + ex1);
            }
        }

        static void Usage()
        {
            Console.WriteLine("Zip periodically files located in a folder, zip names are based on the current timestamp.");
            Console.WriteLine();
            Console.WriteLine("zipgo [-i <Input>] [-o <Output>] [-p <SearchPattern>] interval");
            Console.WriteLine();
            Console.WriteLine(" -Interval : frequency of checking for input files, value is in minutes");
            Console.WriteLine(" -i Input  : input folder, default value is current directory");
            Console.WriteLine(" -o Output : output folder, default value is current directory");
            Console.WriteLine(" -p SearchPattern : the search string to match against the file name");
            Console.WriteLine("                    You can use wildcard * and ?, default value is *.*");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("1 - zipgo 5 -o G:\\tmp");
            Console.WriteLine("2 - zipgo 5 -i G:\\input -o G:\\ouput -p *.log ");
            Console.WriteLine("3 - zipgo 10 -p DB*.log");

        }

        static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}
