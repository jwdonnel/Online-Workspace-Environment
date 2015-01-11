using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;

/// <summary>
/// Summary description for About
/// </summary>
public class About {
    private string _file = "";
    private string _currentVer = "N/A";
    private Dictionary<string, string> _aboutItems = new Dictionary<string, string>();

    public About(string file) {
        _file = file;
    }

    public void ParseXml() {
        try {
            using (XmlReader reader = XmlReader.Create(_file)) {
                int dup = 1;
                bool startParsing = false;
                while (reader.Read()) {
                    string name = reader.Name;
                    if (startParsing) {
                        if ((!string.IsNullOrEmpty(name)) && (name.ToLower() != "items")) {
                            string key = name;
                            if (reader.AttributeCount > 0) {
                                try {
                                    key = reader.GetAttribute("name");
                                    if (string.IsNullOrEmpty(key))
                                        key = name;
                                }
                                catch { }
                            }

                            string value = reader.ReadInnerXml();
                            if (_aboutItems.ContainsKey(key)) {
                                key += " (" + dup.ToString() + ")";
                                dup++;
                            }

                            if (key.ToLower() == "version") {
                                _currentVer = value;
                            }
                            else {
                                _aboutItems.Add(key, value);
                            }
                        }
                    }

                    if (name.ToLower() == "items")
                        startParsing = true;
                }
            }
        }
        catch { }
    }

    #region Get Outputs
    public Dictionary<string, string> AboutItems {
        get { return _aboutItems; }
    }

    public string CurrentVersion {
        get { return _currentVer; }
    }
    #endregion
}