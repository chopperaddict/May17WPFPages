using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WPFPages.ViewModels;

namespace WPFPages.Views
{
	class JsonSupport
	{
		// Class to handle JSN formatting of data
		// 22/6/21 OK
		#region JSON SUPPORT
		/// <summary>
		/// Save in Json format any form of Dbdata (revd as object) to file name as passed in to the Fn
		/// </summary>
		/// <param name="path"></param>
		/// <param name="DbData"></param>
		/// <returns></returns>
		public static string CreateDbToJsonString(string path, object DbData)
		{
			string output = "";

			if (DbData == null || path == "") return "";

			JsonSerializer serializer = new JsonSerializer();
			serializer.Converters.Add(new JavaScriptDateTimeConverter());
			serializer.NullValueHandling = NullValueHandling.Ignore;

			using (StreamWriter sw = new StreamWriter(path))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				serializer.Serialize(writer, DbData);
			}

			return output;

		}

		public static void JsonSerialize(object obj, string filePath)
		{
			var serializer = new JsonSerializer();

			using (var sw = new StreamWriter(filePath))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				serializer.Serialize(writer, obj);
			}
		}

		public static object JsonDeserialize(string path)
		{
			var serializer = new JsonSerializer();

			using (var sw = new StreamReader(path))
			using (var reader = new JsonTextReader(sw))
			{
				return serializer.Deserialize(reader);
			}
		}

		public static string CreateFormattedJsonOutput(string jsonInput, string Title, Progressbar pbar = null)
		{
			int rows = 0, max = 0;
			int fiddle = 0, maxrows = 0;
			double current = 0; maxrows = 0;
			string Output = "", interim = "";
			if (Title.Length > 0)
				Output = "{\r\n\t\"" + Title + " Database Contents\" :\r\n\t{\r\n\t\t";
			else
				Output = "{\r\n\t\t";
			bool isStruct = false;
			// using a stringbuilder improves speed by around 1000 % - honestly !!!
			// it is now almost instant
			StringBuilder sb = new StringBuilder();
			string temp = "";
			string[] tmp1;
			string[] tmp2;
			string[] tmp3;
			string[] tmp4;
			char c;

			// always start with left brace
//			Output += "{\n";
			// split the whole thing up into {} sections
			tmp1 = jsonInput.Split('}');
			max = tmp1.Length-1;
			// tmp1 now has an array of 'n' rows of records
			for (int row = 0; row < tmp1.Length; row++)
			{
				// TEMPORARY EXIT for testing only
//				if (row >= 5) break;

				if (tmp1[row].Length < 3) continue;
				string substr = tmp1[row].Substring(2);
				if (substr.Contains("{") == false)
				{
					//This creates VALID JSON Pretty Human (Readable) Code from Bank Records (all standard records)
					// Ordinary data row
					if(row == 0)
						Output += "\"" + $"{Title}\" : [\n\t" + "\t{\n";
					else
						Output += "\t\t\"" + $"{Title}\" : [\n\t" + "\t{\n";
					Output += ParseRecordString(tmp1[row]);
					// strip off final comma
					interim = Output.Substring(0, Output.Length - 3);
					Output = interim;
					Output += "\n\t\t\t}\n\t\t],\n";
					if (row == max - 4)
					{
						string tmp = Output.Substring(0, Output.Length - 3);
						Output = tmp;
					}
					sb.Append(Output);
					Output = "";
				}
				else
				{
					// It is a Structure of some sort
					// got a structure : [{"Integervalues":{"Age":0,"Rating":0,"Hobby":null
					// parse out  to find the struct name first
					tmp2 = tmp1[row].Split('{');
					if (row == 0)
						Output += tmp2[1] + " [\n\t\t{\n\t\t\t";
					else
						Output += "\t\t" + tmp2[1] + " [\n\t\t{\n\t\t\t";
					Output += ParseStructure(tmp2[2]);
					if (tmp1[row + 1].Contains("["))
					{
						// got another structure
						if (row == max)
							Output += "\n\t";
						else
							Output += ",\n\t";
					}
					else
					{
						// next row is a record, so terminate the structure
//						Output += "\t],";
						Output += "\t\t],\n";
					}
					sb.Append(Output);
					Output = "";
				}
			}
			Output = sb.ToString();
			string tmp22 = Output.Substring(0, Output.Length - 4);
			Output = tmp22 + "\t]\n\t}\n}\n";
			sb.Clear();
			sb.Append(Output);
			File.WriteAllText(@"C:\tmp\xx.json", sb.ToString());
			return sb.ToString(); ;
		}

		private static string ParseStructure(string input)
		{
			string Output = "";
			char c;

			//			Output += "\t";
			for (int x = 0; x < input.Length; x++)
			{
				c = input[x];
				if (c == ',')
					Output += c + "\n\t\t\t";
				else
					Output += c;
			}
			Output += "\n\t\t}\n";
			return Output;
		}

		private static string ParseRecordString(string input)
		{
			string Output = "\t\t\t";
			char c;
			for (int x = 1; x < input.Length; x++)
			{
				c = input[x];
				if (c != '{')
				{
					if (c == ',')
						Output += c + "\r\n\t\t\t";
					else
						Output += c;
				}
			}
			Output += ",\r\n";
			return Output;
		}
		public static StringBuilder CreateJsonFileFromJsonObject(object JsonObject, out string output)
		{
			BankAccountViewModel bvm = new BankAccountViewModel();
			StringBuilder sb = new StringBuilder();
			//			JObject obj = JObject . FromObject ( JsonObject );
			string s = JsonConvert.SerializeObject(new { JsonObject });
			sb.Append(s);
			output = s;
			return sb;
		}

		public static string  CreateShowJsonText(bool Mbox, string CurrentDb, object collection, string Title = "")
		{
			object DbData = new object();
			string resultString = "", path = "";
			string jsonresult = "";

			Progressbar pbar = new Progressbar();
			pbar.Show();
			Mouse.OverrideCursor = Cursors.Wait;

			//We need to save current Collectionview as a Json (binary) data to disk
			// this is the best way to save persistent data in Json format
			//using tmp folder for interim file that we will then display
			if (CurrentDb == "BANKACCOUNT")
			{
				path = @"C:\\tmp\\BankTempdata.json";
				jsonresult = JsonConvert.SerializeObject(collection);
				JsonSupport.JsonSerialize(jsonresult, path);
				Title = "Bank Account";
				//Now Create JSON file in PRETTY FORMAT ??
				resultString = JsonSupport.CreateFormattedJsonOutput(jsonresult, Title, pbar);
			}
			else if (CurrentDb == "CUSTOMER")
			{
				path = @"C:\\tmp\\CustomerTempdata.json";
				jsonresult = JsonConvert.SerializeObject(collection);
				JsonSupport.JsonSerialize(jsonresult, path);
				Title = "Customer Account";
				//Now Create JSON file in PRETTY FORMAT ??
				resultString = JsonSupport.CreateFormattedJsonOutput(jsonresult, Title, pbar);
			}
			else if (CurrentDb == "DETAILS")
			{
				path = @"C:\\tmp\\DetailsTempdata.json";
				jsonresult = JsonConvert.SerializeObject(collection);
				JsonSupport.JsonSerialize(jsonresult, path);
				Title = "Secondary Account";
				//Now Create JSON file in PRETTY FORMAT ??
				resultString = JsonSupport.CreateFormattedJsonOutput(jsonresult, Title, pbar);
			}

			if (Mbox)
			{
				Mouse.OverrideCursor = Cursors.Arrow;
				pbar.Close();
				return resultString;
			}
			// remove tmp file
			File.Delete(path);
			path = @"C:\tmp\dboutput.json";
			File.WriteAllText(path, resultString);
			Mouse.OverrideCursor = Cursors.Arrow;
			pbar.Close();
			/// Finally - Use the default viewer to Display the data we have generated...

			// Get default text files viewer application from App resources
			string program = (string)Properties.Settings.Default["DefaultTextviewer"];

			Process ExternalProcess = new Process();
			ExternalProcess.StartInfo.FileName = program.Trim();
			ExternalProcess.StartInfo.Arguments = path.Trim();
			try
			{
				ExternalProcess.Start();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"ExternalProcess error : {ex.Message}, {ex.Data}");
				if (ex.Message.Contains("cannot find the file"))
				{
					if (Flags.SingleSearchPath != "")
						MessageBox.Show($"Error executing the (Full specified command) \n \"{ program}\"\n\nThe System was unable to Execute this file.", "File Execution Error !");
					else
						MessageBox.Show($"Error executing the (command) shown below\n \"{ program} {path}\"\n\nThe System was unable to Execute this file.", "File Execution Error !");
				}
				else
					MessageBox.Show($"Error executing the (Command) shown below\n \"{ program} {path}\"\n\nSee the Debug output for more information.", "File Execution Error !");

			}
			finally
			{
				ExternalProcess.Close();
			}
			return resultString;
		}
		#endregion JSON SUPPORT

	}
}
