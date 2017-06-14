using PersonalDiscordBot.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PersonalDiscordBot.Classes
{
    public static class Extensions
    {
        public static void uDebugAddLogExternal(string _log)
        {
            string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
            string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
            Toolbox.uDebugAddLog(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
        }
        public static void AddToDebugLog(this string _log)
        {
            string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
            string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
            Toolbox.uDebugAddLog(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
        }

        public static void VerifyXMLNodeAttributes(this XmlNode node, string attribute)
        {
            if (node.Attributes[attribute] == null)
            {
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(attribute);
                node.Attributes.Append(attr);
                node.Attributes[attribute].Value = "";
                uDebugAddLogExternal(string.Format("Added missing {0} attribute to XMLNode {1}", attribute, node.Name));
            }
            node.OwnerDocument.Save(MainWindow._paths.ServerConfig);
        }

        public static void VerifyXMLNodeAttributes(this XmlNode node, string[] attributes)
        {
            foreach (var attribute in attributes)
                if (node.Attributes[attribute] == null)
                {
                    XmlAttribute attr = node.OwnerDocument.CreateAttribute(attribute);
                    node.Attributes.Append(attr);
                    node.Attributes[attribute].Value = "";
                    uDebugAddLogExternal(string.Format("Added missing {0} attribute to XMLNode {1}", attribute, node.Name));
                }
            node.OwnerDocument.Save(MainWindow._paths.ServerConfig);
        }

        public static string ToUpperFirst(this string str)
        {
            if (str.Length <= 0 || str == null)
                return null;
            else if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);
            else
                return str.ToUpper();
        }

        public static string ToUpperAllFirst(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
        }

        public static string ReturnType(this object obj)
        {
            return obj.GetType().ToString();
        }
    }

    public class PromptArgs : EventArgs
    {
        public delegate void MessageShown(PromptArgs args);
        public static event MessageShown MessagePromptShown;
        private string content;
        public PromptArgs(string msgContent)
        {
            this.content = msgContent;
        }
        public string Content { get { return content; } }
        public static void uStatusExtUpdate(string update)
        {
            PromptArgs args = new PromptArgs(update);
            MessagePromptShown(args);
        }
    }

}
