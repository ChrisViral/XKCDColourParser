using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using static System.Console;
using static System.Math;
using static System.Environment;

namespace XKCDColourParser
{
    public class Program
    {
        /// <summary>
        /// Colour support structure 
        /// </summary>
        public readonly struct Colour : IComparable<Colour>, IEquatable<Colour>
        {
            #region Properties
            /// <summary>
            /// Colour name
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Red component
            /// </summary>
            public float R { get; }
            /// <summary>
            /// Green component
            /// </summary>
            public float G { get; }
            /// <summary>
            /// Blue component
            /// </summary>
            public float B { get; }
            /// <summary>
            /// Hue
            /// </summary>
            public float H { get; }
            /// <summary>
            /// Saturation
            /// </summary>
            public float S { get; }
            /// <summary>
            /// Value
            /// </summary>
            public float V { get; }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new colour from a hex string
            /// </summary>
            /// <param name="name">Name of the colour</param>
            /// <param name="hex">Hex string</param>
            public Colour(string name, string hex) : this(name, HexToFloat(hex, 0), HexToFloat(hex, 2), HexToFloat(hex, 4)) { }

            /// <summary>
            /// Creates a new Colour
            /// </summary>
            /// <param name="name">Name of the colour</param>
            /// <param name="r">Red component (0 to 1)</param>
            /// <param name="g">Green component (0 to 1)</param>
            /// <param name="b">Blue component (0 to 1)</param>
            public Colour(string name, float r, float g, float b)
            {
                //RGB
                this.Name = name;
                this.R = Clamp01(r);
                this.G = Clamp01(g);
                this.B = Clamp01(b);

                //HSV data
                float max = Max(this.R, Max(this.G, this.B));
                float min = Min(this.R, Min(this.G, this.B));
                float delta = max - min;

                //Value
                this.V = max;

                //Black
                if (max <= 0f)
                {
                    this.S = 0f;
                    this.H = 0f;
                    return;
                }

                //Saturation
                this.S = delta / max;

                //Hue
                if (this.R >= max) { this.H = (this.G - this.B) / delta; }
                else if (this.G >= max) { this.H = 2f + ((this.B - this.R) / delta); }
                else { this.H = 4f + ((this.R - this.G) / delta); }

                //Make sure we stay in the [0, 360] range
                this.H *= 60f;
                if (this.H <= 0f) { this.H += 360f; }
            }
            #endregion

            #region Static methods
            /// <summary>
            /// Clamps a float value between 0 and 1
            /// </summary>
            /// <param name="f">Float to clamp</param>
            /// <returns>The clamped <paramref name="f"/></returns>
            public static float Clamp01(float f) => Max(0f, Min(1f, f));

            /// <summary>
            /// Converts a hex byte value to a valid 0-1 float representation
            /// </summary>
            /// <param name="hex">Hex string</param>
            /// <param name="start">Start index</param>
            /// <returns>The produced float representation of the hex byte value</returns>
            public static float HexToFloat(string hex, int start) => Convert.ToByte(hex.Substring(start, 2), 16) / 255f;
            #endregion

            #region Methods
            /// <summary>
            /// Compares two colours, using their HSV colour components, sorting them in decreasing order of H, S, and V, in this order
            /// </summary>
            /// <param name="other">Colour to compare to</param>
            /// <returns>-1 if this colour comes before, 1 if it comes after, and 0 if they are equal</returns>
            public int CompareTo(Colour other)
            {
                int compare = other.H.CompareTo(this.H);
                if (compare == 0)
                {
                    compare = other.S.CompareTo(this.S);
                    if (compare == 0)
                    {
                        compare = other.V.CompareTo(this.V);
                    }
                }

                return compare;
            }

            /// <summary>
            /// Creates a nicely formatted string version of this colour's RGB values
            /// </summary>
            /// <returns>String representation of the colour</returns>
            public override string ToString() => $"({this.R}, {this.G}, {this.B})";

            /// <summary>
            /// Creates a nicely formatted string version of this colour's RGB values, with the optional use of the float 'f' modifier for code usage
            /// </summary>
            /// <param name="useModifier">If the 'f' character should be appended after each value or not</param>
            /// <returns>String representation of the colour</returns>
            public string ToString(bool useModifier) => $"({this.R}{(useModifier ? "f" : string.Empty)}, {this.G}{(useModifier ? "f" : string.Empty)}, {this.B}{(useModifier ? "f" : string.Empty)})";

            /// <summary>
            /// Tests if the passed object is a valid equal Colour object
            /// </summary>
            /// <param name="other">Object to test for equality</param>
            /// <returns>True if the other object is a valid and equal Colour object, false otherwise</returns>
            public override bool Equals(object other) => other is Colour c && Equals(c);

            /// <summary>
            /// Gets the Hashcode of this colour, based on the name
            /// </summary>
            /// <returns>Hashcode of this colour</returns>
            public override int GetHashCode() => this.Name?.GetHashCode() ?? 0;

            /// <summary>
            /// Tests to see if the other Colour is equal to this one, based on name equality
            /// </summary>
            /// <param name="other">Other Colour to compare to</param>
            /// <returns>True if both colours are name equivalent, false otherwise</returns>
            public bool Equals(Colour other) => this.Name == other.Name;
            #endregion
        }

        #region Main
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">The file to parse should be arg0</param>
        private static void Main(string[] args)
        {
            //Startup
            WriteLine("\n==== Starting XKCD colour parser ====\n");
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Quit("No file provided, or provided file is invalid, press any key to exit...", 2); //File not found exit code
            }

            //Get file
            string path = args[0];
            WriteLine("Loading from " + path);
            Stopwatch watch = Stopwatch.StartNew();
            string[] lines = File.ReadAllLines(path);
            WriteLine($"Loaded, {lines.Length} lines to parse");
            
            //Parse colours
            SortedSet<Colour> colours = new SortedSet<Colour>();
            foreach (string t in lines)
            {
                string line = t.Trim();
                if (string.IsNullOrWhiteSpace(line) || line[0] == '#') { continue; } //Empty or comment

                string[] splits = line.Split(' ');
                if (splits.Length < 2) { continue; }                                 //Badly formatted

                string name = splits[0]?.Trim();
                if (string.IsNullOrEmpty(name)) { continue; }                        //Badly formatted

                string colour = splits[1]?.Trim();
                if (string.IsNullOrEmpty(colour) || colour.Length < 6) { continue; } //Badly formatted

                colours.Add(new Colour(name, colour));
            }
            watch.Stop();

            //Get formatting info
            WriteLine($"Detected {colours.Count} correctly formatted colours, in {watch.ElapsedMilliseconds}ms");
            bool cs6 = GetConsoleInput("Do you want to format the file for C# 6.0 and up? (y/n)", "y", "n") == "y";
            bool genDict = GetConsoleInput("Generate name->colour map dictionary and associated methods?", "y", "n") == "y";
            WriteLine("Press any key to proceed with parsing...");
            ReadKey();

            //Start formatter
            watch.Restart();
            StringBuilder sb = new StringBuilder(colours.Count * (genDict ? 1 : 2) * 250);
            sb.AppendLine("using UnityEngine;");
            if (genDict) { sb.AppendLine("using System.Collections.Generic;"); }
            sb.AppendLine($"{NewLine}namespace XKCD{NewLine}{{");
            sb.AppendLine($"    public static class XKCDColours{NewLine}    {{");

            //Important vars
            const string ident = "        ";
            const string startSummary = ident + "/// <summary>";
            const string endSummary   = ident + "/// </summary>";
            int i = 0;

            //Colour variables
            sb.AppendLine(ident + "#region Colours");
            foreach (Colour colour in colours)
            {
                WriteLine($"Creating variable for colour {colour.Name}");
                sb.AppendLine(startSummary);
                sb.AppendLine($"{ident}/// A formatted XKCD survey colour {colour}");
                sb.AppendLine(endSummary);
                sb.AppendLine($"{ident}public static {(cs6 ? $"Color {colour.Name} {{ get; }}" : $"readonly Color {colour.Name}")} = new Color{colour.ToString(true)};{(++i == colours.Count ? string.Empty : NewLine)}");
            }
            sb.AppendLine(ident + "#endregion");

            //Conversion dictionary generation
            if (genDict)
            {
                //More important variables
                const string dictIdent = ident + "    ";
                i = 0;

                //Dictionary
                sb.AppendLine(NewLine + ident + "#region Map");
                sb.AppendLine(startSummary);
                sb.AppendLine(ident + "/// A name to colour parsing dictionary");
                sb.AppendLine(endSummary);
                sb.AppendLine($"{ident}private static readonly Dictionary<string, Color> stringToColour = new Dictionary<string, Color>({colours.Count}){NewLine}{ident}{{");
                foreach (Colour colour in colours)
                {
                    WriteLine($"Generating dictionary entry for colour {colour.Name}");
                    sb.AppendLine($"{dictIdent}{(cs6 ? $"[\"{colour.Name}\"] = {colour.Name}" : $"{{ \"{colour.Name}\", {colour.Name} }}")}{(++i == colours.Count ? string.Empty : ",")}");
                }

                //GetColour method
                sb.AppendLine(ident + "};" + NewLine);
                sb.AppendLine(startSummary);
                sb.AppendLine(ident + "/// Gets an XKCDColour from it's name");
                sb.AppendLine(endSummary);
                sb.AppendLine(ident + "/// <param name=\"name\">Name of the colour to get</param>");
                sb.AppendLine(ident + "/// <returns>The found colour of the given name</returns>");
                sb.AppendLine($"{ident}public static Color GetColor(string name) {(cs6 ? "=> stringToColour[name];" : "{ return stringToColour[name]; }")}{NewLine}");

                //TryGetColour method
                sb.AppendLine(startSummary);
                sb.AppendLine(ident + "/// Tries and gets an XKCDColour from it's name, and stores it in the out parameter");
                sb.AppendLine(endSummary);
                sb.AppendLine(ident + "/// <param name=\"name\">Name of the colour to get</param>");
                sb.AppendLine(ident + "/// <param name=\"colour\">Color to store the result in</param>");
                sb.AppendLine(ident + "/// <returns>True if the colour of the given name was found, false otherwise</returns>");
                sb.AppendLine($"{ident}public static bool TryGetColor(string name, out Color colour) {(cs6 ? "=> stringToColour.TryGetValue(name, out colour);" : "{ return stringToColour.TryGetValue(name, out colour); }")}");
                sb.AppendLine(ident + "#endregion");
            }

            //Wrap up class
            sb.AppendLine("    }");
            sb.Append('}');

            //Save to disk
            WriteLine("Generation complete, saving as \"XKCDColours.cs\"");
            string savePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "XKCDColours.cs");
            File.WriteAllText(savePath, sb.ToString());
            watch.Stop();
            WriteLine($"Parsed in  {watch.ElapsedMilliseconds}ms");
            Quit();
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Gets a character input from the console, restricted to a set of specified characters
        /// </summary>
        /// <param name="message">Message to write to the console</param>
        /// <param name="values">Accepted input character values</param>
        /// <returns>The valid registered input value</returns>
        private static string GetConsoleInput(string message, params string[] values)
        {
            string answer = null;
            while (answer == null)
            {
                WriteLine(message);
                string value = ReadLine();
                int index = Array.IndexOf(values, value);
                if (index == -1)
                {
                    WriteLine("Inputted character invalid");
                }
                else { answer = value; }
            }
            return answer;
        }

        /// <summary>
        /// Displays a closing message and closes the command line
        /// </summary>
        /// <param name="message">Exit message to pass out</param>
        /// <param name="code">Exit code</param>
        private static void Quit(string message = "Press any key to exit...", int code = 0)
        {
            
            WriteLine(message);
            ReadKey(true);
            Exit(code);
        }
        #endregion
    }
}