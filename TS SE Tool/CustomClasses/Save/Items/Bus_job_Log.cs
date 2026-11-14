using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS_SE_Tool.Utilities;
using TS_SE_Tool.Save.DataFormat;

namespace TS_SE_Tool.Save.Items
{
    class Bus_job_Log : SiiNBlockCore
    {
        internal int version { get; set; } = 0;

        internal List<string> entries { get; set; } = new List<string>();

        internal Bus_job_Log()
        { }

        internal Bus_job_Log(string[] _input)
        {
            string tagLine = "", dataLine = "";

            foreach (string currentLine in _input)
            {
                if (currentLine.Contains(':'))
                {
                    string[] splittedLine = currentLine.Split(new char[] { ':' }, 2);

                    tagLine = splittedLine[0].Trim();
                    dataLine = splittedLine[1].Trim();
                }
                else
                {
                    tagLine = currentLine.Trim();
                    dataLine = "";
                }

                try
                {
                    switch (tagLine)
                    {
                        case "":
                        case "bus_job_log":
                        case "}":
                            {
                                break;
                            }

                        case "version":
                            {
                                version = int.Parse(dataLine);
                                break;
                            }

                        case "entries":
                            {
                                entries.Capacity = int.Parse(dataLine);
                                break;
                            }

                        case var s when s.StartsWith("entries["):
                            {
                                entries.Add(dataLine);
                                break;
                            }

                        default:
                            {
                                UnidentifiedLines.Add(currentLine);
                                IO_Utilities.ErrorLogWriter(WriteErrorMsg(tagLine, dataLine));
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    IO_Utilities.ErrorLogWriter(WriteErrorMsg(ex.Message, tagLine, dataLine));
                    continue;
                }
            }
        }

        internal string PrintOut(uint _version)
        {
            return PrintOut(_version, null);
        }
        internal string PrintOut(uint _version, string _nameless)
        {
            string returnString = "";

            StringBuilder returnSB = new StringBuilder();

            returnSB.AppendLine("bus_job_log : " + _nameless + " {");

            returnSB.AppendLine(" version: " + version.ToString());

            returnSB.AppendLine(" entries: " + entries.Count);
            for (int i = 0; i < entries.Count; i++)
                returnSB.AppendLine(" entries[" + i + "]: " + entries[i]);

            returnSB.Append(WriteUnidentifiedLines());

            returnSB.AppendLine("}");

            returnString = returnSB.ToString();

            this.removeWritenBlock(_nameless);

            return returnString;
        }
    }

    // Coach job history entries are referenced by Bus_job_Log.entries. Keeping them
    // typed prevents the serializer from also writing them as unidentified blocks.
    class Bus_job_Log_Entry : SiiNBlockCore
    {
        internal int flags { get; set; } = 0;
        internal string type { get; set; } = "";
        internal string assigned_bus { get; set; } = "";
        internal string source_bus_stop_name { get; set; } = "";
        internal string target_bus_stop_name { get; set; } = "";
        internal int stage_count { get; set; } = 0;
        internal int exp_delta { get; set; } = 0;
        internal int reward { get; set; } = 0;
        internal int travelled_distance { get; set; } = 0;
        internal int start_time { get; set; } = 0;
        internal int end_time { get; set; } = 0;
        internal int total_passengers { get; set; } = 0;
        internal SCS_Float satisfaction { get; set; } = 0;

        internal Bus_job_Log_Entry(string[] _input)
        {
            foreach (string currentLine in _input)
            {
                if (currentLine.Contains(':'))
                {
                    string[] split = currentLine.Split(new char[] { ':' }, 2);
                    string tag = split[0].Trim();
                    string value = split[1].Trim();

                    try
                    {
                        switch (tag)
                        {
                            case "":
                            case "bus_job_log_entry":
                            case "}":
                                break;
                            case "flags": flags = int.Parse(value); break;
                            case "type": type = value; break;
                            case "assigned_bus": assigned_bus = value; break;
                            case "source_bus_stop_name": source_bus_stop_name = value; break;
                            case "target_bus_stop_name": target_bus_stop_name = value; break;
                            case "stage_count": stage_count = int.Parse(value); break;
                            case "exp_delta": exp_delta = int.Parse(value); break;
                            case "reward": reward = int.Parse(value); break;
                            case "travelled_distance": travelled_distance = int.Parse(value); break;
                            case "start_time": start_time = int.Parse(value); break;
                            case "end_time": end_time = int.Parse(value); break;
                            case "total_passengers": total_passengers = int.Parse(value); break;
                            case "satisfaction": satisfaction = value; break;
                            default:
                                UnidentifiedLines.Add(currentLine);
                                IO_Utilities.ErrorLogWriter(WriteErrorMsg(tag, value));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        IO_Utilities.ErrorLogWriter(WriteErrorMsg(ex.Message, tag, value));
                    }
                }
                else if (currentLine.Trim() != "bus_job_log_entry" && currentLine.Trim() != "}")
                {
                    UnidentifiedLines.Add(currentLine);
                }
            }
        }

        internal string PrintOut(uint _version, string _nameless)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("bus_job_log_entry : " + _nameless + " {");
            output.AppendLine(" flags: " + flags);
            output.AppendLine(" type: " + type);
            output.AppendLine(" assigned_bus: " + assigned_bus);
            output.AppendLine(" source_bus_stop_name: " + source_bus_stop_name);
            output.AppendLine(" target_bus_stop_name: " + target_bus_stop_name);
            output.AppendLine(" stage_count: " + stage_count);
            output.AppendLine(" exp_delta: " + exp_delta);
            output.AppendLine(" reward: " + reward);
            output.AppendLine(" travelled_distance: " + travelled_distance);
            output.AppendLine(" start_time: " + start_time);
            output.AppendLine(" end_time: " + end_time);
            output.AppendLine(" total_passengers: " + total_passengers);
            output.AppendLine(" satisfaction: " + satisfaction.ToString());
            output.Append(WriteUnidentifiedLines());
            output.AppendLine("}");
            removeWritenBlock(_nameless);
            return output.ToString();
        }
    }
}
