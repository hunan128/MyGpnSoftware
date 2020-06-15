using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace StcCSharp
{
    public class Stc
    {        
        public static void Init()
        {
            string installDir = System.Environment.GetEnvironmentVariable("STC_DIR");

            if (String.IsNullOrEmpty(installDir))
            {
                throw new ApplicationException("STC_DIR environment variable is not defined. It should be set to the STC install directory.");
            }

            string iniFile = Path.Combine(installDir, "stcbll.ini");
            if(File.Exists(iniFile) == false)
            {
                throw new Exception(installDir + " is not a valid STC install directory.");
            }
            
            System.Environment.SetEnvironmentVariable("STC_PRIVATE_INSTALL_DIR", installDir);            
            System.Environment.SetEnvironmentVariable("Path", 
                System.Environment.GetEnvironmentVariable("Path") + ";" + installDir);
            
            StcIntCSharp.stcIntCSharpInit();
        }

        public static void Log(string logLevel, string message) 
        {
		    StcIntCSharp.salLog(logLevel, message);
	    }

	    public static void Shutdown() 
        {
		    StcIntCSharp.salShutdown();
	    }

        public static void Connect(string hostName) 
        {
		    StringVector sv = new StringVector();           
		    sv.Add(hostName);
		    StcIntCSharp.salConnect(sv);
	    }

        public static void Connect(List<string> hostNames) 
        {
		    StringVector sv = new StringVector(hostNames);            
		    StcIntCSharp.salConnect(sv);
	    }

        public static void Disconnect(string hostName)
        {
            StringVector sv = new StringVector();
            sv.Add(hostName);
            StcIntCSharp.salDisconnect(sv);
        }

        public static void Disconnect(List<string> hostNames)
        {
		    StringVector sv = new StringVector(hostNames);		    
		    StcIntCSharp.salDisconnect(sv);
	    }

        public static string Create(string type, string parent)
        {
            StringVector sv = new StringVector();
            sv.Add("-under");
            sv.Add(parent);
            return StcIntCSharp.salCreate(type, sv);
        }

        public static string Create(string type, string parent, Dictionary<string, string> propertyPairs)
        {
            StringVector sv = new StringVector();
            sv.Add("-under");
            sv.Add(parent);
            MapToStringVector(propertyPairs, sv);
            return StcIntCSharp.salCreate(type, sv);
        }

        public static void Delete(string handle) 
        {
		    StcIntCSharp.salDelete(handle);
	    }

        public static void Config(string handle, string attribName, string attribValue) 
        {
		    StringVector sv = new StringVector();
		    sv.Add("-" + attribName);
		    sv.Add(attribValue);
		    StcIntCSharp.salSet(handle, sv);
	    }

	    public static void Config(string handle, Dictionary<string, string> propertyPairs) 
        {
		    StringVector sv = new StringVector();
		    MapToStringVector(propertyPairs, sv);
		    StcIntCSharp.salSet(handle, sv);
	    }

        public static Dictionary<string, string> Get(string handle)
        {
            return StringVectorToMap(StcIntCSharp.salGet(handle, new StringVector()));
        }

        public static string Get(string handle, string property)
        {
            StringVector sv = new StringVector();
            sv.Add("-" + property);
            StringVector sv2 = StcIntCSharp.salGet(handle, sv);
            return sv2[0];
        }

        public static Dictionary<string, string> Get(string handle, List<string> properties)
        {
            StringVector sv = StcIntCSharp.salGet(handle, StringListToStringVector(properties));
            return UnpackGetResponseAndReturnKeyVal(sv, properties);  
        }
        
        public static Dictionary<string, string> Perform(string commandName) 
        {
		    return StringVectorToMap(StcIntCSharp.salPerform(commandName, new StringVector()));
	    }

        public static Dictionary<string, string> Perform(string commandName, Dictionary<string, string> propertyPairs) 
        {
		    StringVector sv = new StringVector();
		    MapToStringVector(propertyPairs, sv);
		    StringVector retSv = StcIntCSharp.salPerform(commandName, sv);
            return UnpackPerformResponseAndReturnKeyVal(retSv, propertyPairs);            		    
	    }   

        public static void Reserve(string CSP) 
        {
		    StringVector sv = new StringVector();
		    sv.Add(CSP);
		    StcIntCSharp.salReserve(sv);
	    }

        public static void Reserve(List<string> CSPs) 
        {
		    StringVector sv = new StringVector(CSPs);		    
		    StcIntCSharp.salReserve(sv);
	    }

        public static void Release(string CSP)
        {
            StringVector sv = new StringVector();
            sv.Add(CSP);
            StcIntCSharp.salRelease(sv);
        }

        public static void Release(List<string> CSPs) 
        {
		    StringVector sv = new StringVector(CSPs);		    
		    StcIntCSharp.salRelease(sv);
	    }

        public static string Subscribe(Dictionary<string, string> inputParameters) 
        {
		    StringVector sv = new StringVector();
		    MapToStringVector(inputParameters, sv);
		    return StcIntCSharp.salSubscribe(sv);
	    }

        public static void Unsubscribe(string handle) 
        {
		    StcIntCSharp.salUnsubscribe(handle);
	    }

        public static void Apply()
        {
            StcIntCSharp.salApply();
        }

        public static string WaitUntilComplete()
        {
            return DoWaitUntilComplete(0);
        }

        public static string WaitUntilComplete(int timeoutInSec)
        {
            return DoWaitUntilComplete(timeoutInSec);
        }

        private static string DoWaitUntilComplete(int timeoutInSec)
        {
            string seq = Stc.Get("system1", "children-sequencer");
            int timer = 0;
            while(true)
            {
                string curTestState = Stc.Get(seq, "state");
                if(curTestState.Equals("PAUSE") || curTestState.Equals("IDLE"))
                {
                    break;
                }
                
                Thread.Sleep(1000);
                timer += 1;
                if(timeoutInSec > 0 && timer > timeoutInSec)
                {                    
                    throw new Exception(String.Format("ERROR: Stc.WaitUntilComplete timed out after {0} sec", timeoutInSec));
                }                
            }

            string syncFiles = System.Environment.GetEnvironmentVariable("STC_SESSION_SYNCFILES_ON_SEQ_COMPLETE");
            if(syncFiles != null && syncFiles.Equals("1") &&
                Stc.Perform("CSGetBllInfo")["ConnectionType"].Equals("SESSION"))
            {
                Stc.Perform("CSSynchronizeFiles");
            }

            return Stc.Get(seq, "testState");
        }                       
        
        internal static Dictionary<String, String> StringVectorToMap(StringVector sv)
        {            
            Dictionary<string, string> sm = new Dictionary<string, string>();
            for (int i = 0; i < sv.Count; i += 2)
            {
                string propName = sv[i].Substring(1, sv[i].Length - 1); //take out the dash
                string propVal = sv[i + 1];
                sm[propName] = propVal;
            }
            return sm;
        }

        internal static StringVector StringListToStringVector(List<string> strings)
        {
            StringVector sv = new StringVector();
            foreach (string s in strings)
            {
                sv.Add("-" + s);
            }            
            return sv;
        }

        internal static void MapToStringVector(Dictionary<string, string> sm, StringVector sv)
        {
            foreach (KeyValuePair<string, string> pair in sm)
            {
                sv.Add("-" + pair.Key);
                sv.Add(pair.Value);
            }            
        }

        internal static Dictionary<string, string> UnpackGetResponseAndReturnKeyVal(StringVector sv, List<string> props)
        {
            Dictionary<string, string> propVals = new Dictionary<string, string>();            
            for(int i = 0; i < (sv.Count/2); ++i)
            {
                string key = props[i];
                string val = sv[i*2 + 1];
                propVals[key] = val;
            }

            return propVals;
        }

        internal static Dictionary<string, string> UnpackPerformResponseAndReturnKeyVal(StringVector sv, Dictionary<string, string> origKeys)
        {
            Dictionary<string, string> origKeyHash = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in origKeys)
            {
                origKeyHash[pair.Key.ToLower()] = pair.Key;
            }
            
            Dictionary<string, string> retVals = new Dictionary<string, string>();
            for(int i = 0; i < (sv.Count / 2); ++i)
            {
                string key = sv[i*2].Substring(1, sv[i*2].Length - 1);
                string val = sv[i*2 + 1];
                if(origKeyHash.ContainsKey(key.ToLower()))
                {
                    key = origKeyHash[key.ToLower()];
                }

                retVals[key] = val;
            }

            return retVals;
        }
    }
}
