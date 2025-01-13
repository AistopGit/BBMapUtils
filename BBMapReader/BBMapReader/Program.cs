using System;

public class BuildingBlock
{
    public enum BuildingBlockColor
    {
        None = 0,
        White = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Red = 5,
        Purple = 6,
        Black = 7,
        Custom = 8
    }
}

class Reader
{
    private static void ShowException(Exception e)
    {
        ConsoleColor currentConsoleColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("An exception occured.\n" + e);
        Console.ForegroundColor = currentConsoleColor;
        Environment.Exit(1);
    }

    private static void MapBinToTxt(FileStream fsr, FileStream fsw)
    {
        using (BinaryReader reader = new BinaryReader(fsr))
        {
            using (StreamWriter writer = new StreamWriter(fsw))
            {
                byte version;
                bool verified;
                string mapName;
                ulong creatorID;
                float bestTime;
                bool usesDLC1Map;

                version = reader.ReadByte();
                verified = reader.ReadBoolean();
                mapName = reader.ReadString();
                creatorID = reader.ReadUInt64();
                bestTime = reader.ReadSingle();

                writer.WriteLine("Version: " + version);
                writer.WriteLine("Verified: " + verified);
                writer.WriteLine("Name: " + mapName);
                writer.WriteLine("Creator ID: " + creatorID);
                writer.WriteLine("Best time: " + bestTime);

                if (version >= 3)
                    usesDLC1Map = reader.ReadBoolean();
                else
                    usesDLC1Map = false;
                writer.WriteLine("Uses DLC1 map: " + usesDLC1Map);
                writer.WriteLine("\n");

                fsr.Seek(128, SeekOrigin.Begin);

                // Blocks
                int numBlocks = reader.ReadInt32();
                writer.WriteLine("Blocks count: " + numBlocks);
                writer.WriteLine("Blocks: \n");

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

                    blockID = reader.ReadUInt32();
                    instanceID = reader.ReadString();
                    color = reader.ReadByte();

                    writer.WriteLine("Block ID: " + blockID);
                    writer.WriteLine("Instance ID: " + instanceID);
                    writer.WriteLine("Color: " + color + " (" + (BuildingBlock.BuildingBlockColor)color + ")");

                    writer.Write("Position: ");
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    writer.Write(x + ", ");
                    writer.Write(y + ", ");
                    writer.WriteLine(z);

                    writer.Write("Rotation: ");
                    x = reader.ReadSingle();
                    y = reader.ReadSingle();
                    z = reader.ReadSingle();
                    w = reader.ReadSingle();
                    writer.Write(x + ", ");
                    writer.Write(y + ", ");
                    writer.Write(z + ", ");
                    writer.WriteLine(w);

                    if (version >= 2)
                    {
                        scale = reader.ReadSingle();
                    }
                    writer.WriteLine("Scale: " + scale);

                    if (version >= 4 && color == 8)
                    {
                        writer.Write("Custom color: ");
                        float[] customColor =
                        [
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                        ];
                        writer.Write(customColor[0] + ", ");
                        writer.Write(customColor[1] + ", ");
                        writer.Write(customColor[2] + ", ");
                        writer.WriteLine(customColor[3]);
                    }

                    writer.WriteLine(); // "\n"
                }

                // Modified sceneries
                if (version >= 3)
                {
                    int numModifiedScenery = reader.ReadInt32();
                    writer.WriteLine("Modified sceneries count: " + numModifiedScenery);
                    for (int i = 0; i < numModifiedScenery; i++)
                    {
                        string sceneryID = reader.ReadString();
                        writer.WriteLine("Scenery ID: " + sceneryID);
                        byte color = reader.ReadByte();
                        writer.WriteLine("Color: " + color + " (" + (BuildingBlock.BuildingBlockColor)color + ")");
                    }
                }

                writer.WriteLine();

                // Custom color swatches
                if (version >= 4)
                {
                    int numCustomColors = reader.ReadInt32();
                    writer.WriteLine("Custom color swatches count: " + numCustomColors);
                    writer.WriteLine();
                    float[] customColorSwatches = new float[numCustomColors * 4];
                    for (int i = numCustomColors * 4 - 1; i >= 0; i -= 4)
                    {
                        writer.WriteLine("Custom color swatch: ");
                        customColorSwatches[i] = reader.ReadSingle();
                        customColorSwatches[i - 1] = reader.ReadSingle();
                        customColorSwatches[i - 2] = reader.ReadSingle();
                        customColorSwatches[i - 3] = reader.ReadSingle();
                        writer.Write(customColorSwatches[i] + ", ");
                        writer.Write(customColorSwatches[i - 1] + ", ");
                        writer.Write(customColorSwatches[i - 2] + ", ");
                        writer.WriteLine(customColorSwatches[i - 3]);
                        writer.WriteLine();
                    }
                }
            }
        }
    }
    public static void Main(string[] args)
    {
        Console.WriteLine("Beton Brutal Map Reader 1.0");

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
                            MapBinToTxt(fsr, fsw);
                            Console.WriteLine("Map successfully written to " + args[1]);
                        }
                    }

                    else
                    {
                        string outFileName = Path.GetFileNameWithoutExtension(args[0]) + ".txt";
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
                            MapBinToTxt(fsr, fsw);
                            Console.WriteLine("Map successfully written to " + outFileName);
                        }
                    }
                }
            }

            else
            {
                Console.WriteLine("The map file name has not been specified, or the specified file does not exist, or cannot be accessed.");
                Console.WriteLine("Usage: BBMapReader <map file name> [output file name]");
                Console.WriteLine("Examples:");
                Console.WriteLine("BBMapReader Map.bbmap");
                Console.WriteLine("BBMapReader Map.bbmap Map.txt");
            }
        }

        catch (Exception e)
        {
            ShowException(e);
        }
    }
}