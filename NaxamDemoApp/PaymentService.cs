/////////////////////////////////////////////////////////////////////////////
// $Id: $
// <copyright file="PaymentService.cs" company="James John McGuire">
// Copyright © 2016 - 2018 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Android.App;
using Android.Content;

namespace NaxamDemoSlim
{
	[Service]
	public static class PaymentService
	{
		public static void SubmitPayment(string clientToken)
		{
			var context = MainActivity.Instance;
			context.AddPaymentMethod();

			//Intent intent = new Intent(
			//	Android.App.Application.Context, typeof(PaymentActivity));
			//intent.PutExtra("ClientToken", clientToken);
			//Android.App.Application.Context.StartActivity(intent);

			//// Needed?
			//Finish();
		}
	}
}
