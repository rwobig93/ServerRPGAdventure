using Discord.Commands;
using Newtonsoft.Json;
using PersonalDiscordBot.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        public static int ToArrayLength(this Array array)
        {
            return array.Length - 1;
        }

    }

    public static class Toolbox
    {
        public static StringBuilder debugLog = new StringBuilder();
        public static Classes.LocalSettings _paths = new Classes.LocalSettings();

        public static void uDebugAddLog(string _log)
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _timeNow = DateTime.Now.ToLocalTime().ToLongTimeString();
                debugLog.AppendLine(string.Format("{0}_{1} :: {2}", _dateNow, _timeNow, _log));
                if (debugLog.Length >= 5000)
                    DumpDebugLog();
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        public static void DumpDebugLog()
        {
            try
            {
                string _dateNow = DateTime.Now.ToLocalTime().ToString("MM-dd-yy");
                string _debugLocation = string.Format(@"{0}\DebugLog_{1}.txt", _paths.LogLocation, _dateNow);
                if (!File.Exists(_debugLocation))
                    using (StreamWriter _sw = new StreamWriter(_debugLocation))
                        _sw.WriteLine(debugLog.ToString());
                else
                    using (StreamWriter _sw = File.AppendText(_debugLocation))
                        _sw.WriteLine(debugLog.ToString());
                debugLog.Clear();
                DirectoryInfo _dI = new DirectoryInfo(_paths.LogLocation);
                foreach (FileInfo _fI in _dI.GetFiles())
                {
                    if (_fI.Name.StartsWith("DebugLog") && _fI.CreationTime.ToLocalTime() <= DateTime.Now.AddDays(-14).ToLocalTime())
                    {
                        _fI.Delete(); uDebugAddLog(string.Format("Deleted old DebugLog: {0}", _fI.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                FullExceptionLog(ex);
            }
        }

        private static void FullExceptionLog(Exception ex, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null)
        {
            string exString = string.Format("TimeStamp: {1}{0}Exception Type: {2}{0}Caller: {3} at {4}{0}Message: {5}{0}HR: {6}{0}StackTrace:{0}{7}{0}", Environment.NewLine, string.Format("{0}_{1}", DateTime.Now.ToLocalTime().ToString("MM-dd-yy"), DateTime.Now.ToLocalTime().ToLongTimeString()), ex.GetType().Name, caller, lineNumber, ex.Message, ex.HResult, ex.StackTrace);
            uDebugAddLog(string.Format("EXCEPTION: {0} at {1}", caller, lineNumber));
            string _logLocation = string.Format(@"{0}\Exceptions.log", _paths.LogLocation);
            if (!File.Exists(_logLocation))
                using (StreamWriter _sw = new StreamWriter(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
            else
                using (StreamWriter _sw = File.AppendText(_logLocation))
                    _sw.WriteLine(exString + Environment.NewLine);
        }

        public static List<RebootedServer> serversRebooted = new List<RebootedServer>();

        public static void RemoveRebootedServer(RebootedServer rebServ)
        {
            try
            {
                Thread remServ = new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(TimeSpan.FromMinutes(15));
                        serversRebooted.Remove(rebServ);
                        string.Format("Removed rebooted server entry for game server {0} after 15 min", rebServ.Server.ServerName).AddToDebugLog();
                    }
                    catch (Exception ex)
                    {
                        ServerModule.FullExceptionLog(ex);
                    }
                });
                remServ.Start();
            }
            catch (Exception ex)
            {
                ServerModule.FullExceptionLog(ex);
            }
        }

        public static void RefreshItemSource(this ListView listView)
        {
            var tempStorage = listView.ItemsSource;
            listView.ItemsSource = null;
            listView.ItemsSource = tempStorage;
        }

        public static bool IsPortOpen(string ipAddress, int portNum)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socket.Connect(ipAddress, portNum);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.ConnectionRefused || ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        return false;
                    }
                }
                return true;
            }
        }


    }

    /// <summary>
    /// Non-generic class allowing properties to be copied from one instance
    /// to another existing instance of a potentially different type.
    /// </summary>
    public static class PropertyCopy
    {
        /// <summary>
        /// Copies all public, readable properties from the source object to the
        /// target. The target type does not have to have a parameterless constructor,
        /// as no new instance needs to be created.
        /// </summary>
        /// <remarks>Only the properties of the source and target types themselves
        /// are taken into account, regardless of the actual types of the arguments.</remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="target">Target to copy properties to</param>
        public static void Copy<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            PropertyCopier<TSource, TTarget>.Copy(source, target);
        }
    }

    /// <summary>
    /// Generic class which copies to its target type from a source
    /// type specified in the Copy method. The types are specified
    /// separately to take advantage of type inference on generic
    /// method arguments.
    /// </summary>
    public static class PropertyCopy<TTarget> where TTarget : class, new()
    {
        /// <summary>
        /// Copies all readable properties from the source to a new instance
        /// of TTarget.
        /// </summary>
        public static TTarget CopyFrom<TSource>(TSource source) where TSource : class
        {
            return PropertyCopier<TSource, TTarget>.Copy(source);
        }
    }

    /// <summary>
    /// Static class to efficiently store the compiled delegate which can
    /// do the copying. We need a bit of work to ensure that exceptions are
    /// appropriately propagated, as the exception is generated at type initialization
    /// time, but we wish it to be thrown as an ArgumentException.
    /// Note that this type we do not have a constructor constraint on TTarget, because
    /// we only use the constructor when we use the form which creates a new instance.
    /// </summary>
    internal static class PropertyCopier<TSource, TTarget>
    {
        /// <summary>
        /// Delegate to create a new instance of the target type given an instance of the
        /// source type. This is a single delegate from an expression tree.
        /// </summary>
        private static readonly Func<TSource, TTarget> creator;

        /// <summary>
        /// List of properties to grab values from. The corresponding targetProperties 
        /// list contains the same properties in the target type. Unfortunately we can't
        /// use expression trees to do this, because we basically need a sequence of statements.
        /// We could build a DynamicMethod, but that's significantly more work :) Please mail
        /// me if you really need this...
        /// </summary>
        private static readonly List<PropertyInfo> sourceProperties = new List<PropertyInfo>();
        private static readonly List<PropertyInfo> targetProperties = new List<PropertyInfo>();
        private static readonly Exception initializationException;

        internal static TTarget Copy(TSource source)
        {
            if (initializationException != null)
            {
                throw initializationException;
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return creator(source);
        }

        internal static void Copy(TSource source, TTarget target)
        {
            if (initializationException != null)
            {
                throw initializationException;
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            for (int i = 0; i < sourceProperties.Count; i++)
            {
                targetProperties[i].SetValue(target, sourceProperties[i].GetValue(source, null), null);
            }

        }

        static PropertyCopier()
        {
            try
            {
                creator = BuildCreator();
                initializationException = null;
            }
            catch (Exception e)
            {
                creator = null;
                initializationException = e;
            }
        }

        private static Func<TSource, TTarget> BuildCreator()
        {
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();
            foreach (PropertyInfo sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sourceProperty.CanRead)
                {
                    continue;
                }
                PropertyInfo targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
                if (targetProperty == null)
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not present and accessible in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.CanWrite)
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not writable in " + typeof(TTarget).FullName);
                }
                if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " is static in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    throw new ArgumentException("Property " + sourceProperty.Name + " has an incompatible type in " + typeof(TTarget).FullName);
                }
                bindings.Add(Expression.Bind(targetProperty, Expression.Property(sourceParameter, sourceProperty)));
                sourceProperties.Add(sourceProperty);
                targetProperties.Add(targetProperty);
            }
            Expression initializer = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
            return Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter).Compile();
        }
    }
}
