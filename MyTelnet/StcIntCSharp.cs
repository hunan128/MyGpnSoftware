/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 1.3.35
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */

namespace Stc {

using System;
using System.Runtime.InteropServices;

public class StcIntCSharp {
  public static void salLog(string logLevel, string msg) {
    StcIntCSharpPINVOKE.salLog(logLevel, msg);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salInit() {
    StcIntCSharpPINVOKE.salInit();
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salShutdown() {
    StcIntCSharpPINVOKE.salShutdown__SWIG_0();
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salShutdown(int exitCode) {
    StcIntCSharpPINVOKE.salShutdown__SWIG_1(exitCode);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salShutdownNoExit() {
    StcIntCSharpPINVOKE.salShutdownNoExit();
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salConnect(StringVector hostNames) {
    StcIntCSharpPINVOKE.salConnect(StringVector.getCPtr(hostNames));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salDisconnect(StringVector hostNames) {
    StcIntCSharpPINVOKE.salDisconnect(StringVector.getCPtr(hostNames));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static string salCreate(string type, StringVector propertyPairs) {
    string ret = StcIntCSharpPINVOKE.salCreate(type, StringVector.getCPtr(propertyPairs));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void salDelete(string handle) {
    StcIntCSharpPINVOKE.salDelete(handle);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void salSet(string handle, StringVector propertyPairs) {
    StcIntCSharpPINVOKE.salSet(handle, StringVector.getCPtr(propertyPairs));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static StringVector salGet(string handle, StringVector propertyNames) {
    StringVector ret = new StringVector(StcIntCSharpPINVOKE.salGet(handle, StringVector.getCPtr(propertyNames)), true);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector salPerform(string commandName, StringVector propertyPairs) {
    StringVector ret = new StringVector(StcIntCSharpPINVOKE.salPerform(commandName, StringVector.getCPtr(propertyPairs)), true);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static StringVector salReserve(StringVector CSPs) {
    StringVector ret = new StringVector(StcIntCSharpPINVOKE.salReserve(StringVector.getCPtr(CSPs)), true);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void salRelease(StringVector CSPs) {
    StcIntCSharpPINVOKE.salRelease(StringVector.getCPtr(CSPs));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static string salSubscribe(StringVector inputParameters) {
    string ret = StcIntCSharpPINVOKE.salSubscribe(StringVector.getCPtr(inputParameters));
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void salUnsubscribe(string handle) {
    StcIntCSharpPINVOKE.salUnsubscribe(handle);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static string salHelp(string info) {
    string ret = StcIntCSharpPINVOKE.salHelp(info);
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public static void salApply() {
    StcIntCSharpPINVOKE.salApply();
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

  public static void stcIntCSharpInit() {
    StcIntCSharpPINVOKE.stcIntCSharpInit();
    if (StcIntCSharpPINVOKE.SWIGPendingException.Pending) throw StcIntCSharpPINVOKE.SWIGPendingException.Retrieve();
  }

}

}
