class Writer
{
    private static void ShowException(Exception e)
    {
        ConsoleColor currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("An exception occured.\n" + e);
        Console.ForegroundColor = currentConsoleColor;
        Environment.Exit(1);
    }

    private static void MapTxtToBin(FileStream fsr, FileStream fsw)
    {
        using (StreamReader reader = new StreamReader(fsr))
        {
            using (BinaryWriter writer = new BinaryWriter(fsw))
            {
                string? currentLine;
                byte version;
                bool verified;
                string mapName;
                ulong creatorID;
                float bestTime;
                bool usesDLC1Map = false;

                currentLine = reader.ReadLine();
                version = Convert.ToByte(currentLine.Substring(9));
                if (version < 1 || version > 4)
                {
                    Console.WriteLine("Wrong version detected: " + version + ". Aborting.");
                    Environment.Exit(1);
                }

                currentLine = reader.ReadLine();
                verified = Convert.ToBoolean(currentLine.Substring(10));

                currentLine = reader.ReadLine();
                mapName = currentLine.Substring(6);
                if (mapName.Length > 113) // Won't fit into the 128B header if bigger than this. Not sure if BB is actually unable to handle this, but I suppose so.
                {
                    Console.WriteLine("The map name is too long, the max length is 113. The current length is: " + mapName.Length + "Aborting.");
                    Environment.Exit(1);
                }

                currentLine = reader.ReadLine();
                creatorID = Convert.ToUInt64(currentLine.Substring(12));

                currentLine = reader.ReadLine();
                bestTime = Convert.ToSingle(currentLine.Substring(10));

                if (version >= 3)
                {
                    currentLine = reader.ReadLine();
                    usesDLC1Map = Convert.ToBoolean(currentLine.Substring(14));
                }

                    writer.Write(version);
                writer.Write(verified);
                writer.Write(mapName);
                writer.Write(creatorID);
                writer.Write(bestTime);
                if (version >= 3)
                {
                    writer.Write(usesDLC1Map);
                }
                writer.Seek(128, SeekOrigin.Begin);

                for (int i = 0; i < 2; i++)
                {
                    reader.ReadLine(); // "\n\n"
                }

                // Blocks
                currentLine = reader.ReadLine();
                int numBlocks = Convert.ToInt32(currentLine.Substring(14));

                writer.Write(numBlocks);
                for (int i = 0; i < 2; i++)
                {
                    reader.ReadLine(); // "Blocks: \n"
                }

                for (int i = 0; i < numBlocks; i++)
                {
                    uint blockID;
                    string instanceID;
                    byte color;
                    float x;
                    float y;
                    float z;
                    float w;
                    float scale = 1.0f;

                    currentLine = reader.ReadLine();
                    blockID = Convert.ToUInt32(currentLine.Substring(10));

                    instanceID = reader.ReadLine().Substring(13);

                    currentLine = reader.ReadLine();
                    color = Convert.ToByte(currentLine.Substring(7, 1));

                    currentLine = reader.ReadLine();
                    currentLine = currentLine.Substring(10, currentLine.Length - 10);

                    /// ++ Position
                    x = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                    currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                    y = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                    currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                    z = Convert.ToSingle(currentLine);

                    writer.Write(blockID);
                    writer.Write(instanceID);
                    writer.Write(color);
                    writer.Write(x);
                    writer.Write(y);
                    writer.Write(z);
                    /// -- Position

                    /// ++ Rotation
                    currentLine = reader.ReadLine();
                    currentLine = currentLine.Substring(10, currentLine.Length - 10);

                    x = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                    currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                    y = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                    currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                    z = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                    currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                    w = Convert.ToSingle(currentLine);

                    writer.Write(x);
                    writer.Write(y);
                    writer.Write(z);
                    writer.Write(w);
                    /// -- Rotation

                    currentLine = reader.ReadLine();
                    if (version >= 2)
                    {
                        scale = Convert.ToSingle(currentLine.Substring(7));
                        writer.Write(scale);
                    }

                    if (version >= 4 && color == 8)
                    {
                        currentLine = reader.ReadLine();
                        currentLine = currentLine.Substring(14, currentLine.Length - 14);

                        x = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        y = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        z = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        w = Convert.ToSingle(currentLine);

                        writer.Write(x);
                        writer.Write(y);
                        writer.Write(z);
                        writer.Write(w);
                    }

                    reader.ReadLine();
                }

                // Modified sceneries. Warning: untested.
                if (version >= 3)
                {
                    currentLine = reader.ReadLine();
                    int numModifiedScenery = Convert.ToInt32(currentLine.Substring(26));
                    writer.Write(numModifiedScenery);

                    for (int i = 0; i < numModifiedScenery; i++)
                    {
                        string sceneryID = reader.ReadLine().Substring(12);

                        currentLine = reader.ReadLine();
                        byte color = Convert.ToByte(currentLine.Substring(7, 1));

                        writer.Write(sceneryID);
                        writer.Write(color);
                    }
                }

                reader.ReadLine();

                // Custom color swatches
                if (version >= 4)
                {
                    currentLine = reader.ReadLine();
                    int numCustomColors = Convert.ToInt32(currentLine.Substring(29));
                    writer.Write(numCustomColors);

                    reader.ReadLine();

                    for (int i = numCustomColors - 1; i >= 0; i--)
                    {
                        reader.ReadLine(); // "Custom color swatch: "
                        currentLine = reader.ReadLine();

                        float x = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        float y = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        float z = Convert.ToSingle(currentLine.Substring(0, currentLine.IndexOf(' ') - 1));
                        currentLine = currentLine.Substring(currentLine.IndexOf(' ') + 1);
                        float w = Convert.ToSingle(currentLine);

                        writer.Write(x);
                        writer.Write(y);
                        writer.Write(z);
                        writer.Write(w);

                        reader.ReadLine();
                    }
                }
            }
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Beton Brutal Map Writer 1.0.1");
        Console.WriteLine("Note: please preserve text formatting (preferrably spaces and empty lines). Not doing so would produce a corrupted map.\n");

        try
        {
            if (args.Length > 0 && File.Exists(args[0]))
            {
                using (FileStream fsr = new FileStream(args[0], FileMode.Open))
                {
                    if (args.Length > 1)
                    {
                        try
                        {
                            File.Create(args[1]).Close();
                        }
                        catch (Exception e)
                        {
                            File.Delete(args[1]);
                            ShowException(e);
                        }
                        using (FileStream fsw = new FileStream(args[1], FileMode.Truncate))
                        {
                            MapTxtToBin(fsr, fsw);
                        }
                    }

                    else
                    {
                        string outFileName = Path.GetFileNameWithoutExtension(args[0]) + ".bbmap";
                        try
                        {
                            File.Create(outFileName).Close();
                        }
                        catch (Exception e)
                        {
                            File.Delete(outFileName);
                            ShowException(e);
                        }
                        using (FileStream fsw = new FileStream(outFileName, FileMode.Truncate))
                        {
                            MapTxtToBin(fsr, fsw);
                        }
                    }
                }
            }

            else
            {
                Console.WriteLine("The input file name has not been specified, or the specified file does not exist, or cannot be accessed.");
                Console.WriteLine("Usage: BBMapWriter <map file name> [output file name]");
                Console.WriteLine("Examples:");
                Console.WriteLine("BBMapWriter Map.txt");
                Console.WriteLine("BBMapWriter Map.txt Map.bbmap");
            }
        }

        catch (Exception e)
        {
            ShowException(e);
        }
    }
}