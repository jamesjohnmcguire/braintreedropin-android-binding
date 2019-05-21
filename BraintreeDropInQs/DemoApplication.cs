﻿using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Lang;
using static Java.Lang.Thread;
//using NaxamDemoCopy.Internal;
using Com.Braintreepayments.Api;

namespace NaxamDemoCopy
{
    [Application]
    public class DemoApplication : Application, IUncaughtExceptionHandler
    {
        private Thread.IUncaughtExceptionHandler mDefaultExceptionHandler;

        public DemoApplication() : base()
        {
            new BraintreeBrowserSwitchActivity();
        }
        protected DemoApplication(IntPtr javaReference, JniHandleOwnership transfer) :base(javaReference, transfer) { }

        public override void OnCreate()
        {
            if (BuildConfig.DEBUG)
            {
                StrictMode.SetThreadPolicy(new StrictMode.ThreadPolicy.Builder()
                        .DetectCustomSlowCalls()
                        .DetectNetwork()
                        .PenaltyLog()
                        .PenaltyDeath()
                        .Build());
                StrictMode.SetVmPolicy(new StrictMode.VmPolicy.Builder()
                        .DetectActivityLeaks()
                        .DetectLeakedClosableObjects()
                        .DetectLeakedRegistrationObjects()
                        .DetectLeakedSqlLiteObjects()
                        .PenaltyLog()
                        .PenaltyDeath()
                        .Build());
            }

            base.OnCreate();

            if (Settings.getVersion(this) != BuildConfig.VERSION_CODE)
            {
                Settings.setVersion(this);
            }

            mDefaultExceptionHandler = Thread.DefaultUncaughtExceptionHandler;
            Thread.DefaultUncaughtExceptionHandler = this;
        }


        public void UncaughtException(Thread t, Throwable e)
        {
            // throw log here

        }
        //public static ApiClient getApiClient(Context context)
        //{
        //    return null;
        //}

        public static void ResetApiClient()
        {
        }
    }
}