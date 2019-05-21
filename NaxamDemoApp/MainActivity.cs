using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using static Android.Support.V4.App.ActivityCompat;
using Com.Braintreepayments.Api.Models;
using Android.Support.V7.Widget;
using Com.Braintreepayments.Api.Exceptions;
using Com.Braintreepayments.Api.Dropin.Utils;
using Com.Braintreepayments.Api.Dropin;
using Android.Gms.Identity.Intents.Model;
using Android.Views;
using Java.Util;
using Com.Braintreepayments.Api;
using System.Collections.Generic;
using Android.Gms.Wallet;
using Android.Content;
using Com.Braintreepayments.Api.Interfaces;
using Java.Lang;
using Android.Runtime;
using Java.Interop;
using System;
using Com.Paypal.Android.Sdk.Onetouch.Core;

namespace NaxamDemoSlim
{
    [Activity(Label = "NaxamDemoSlim", MainLauncher = true, Theme = "@style/Theme.AppCompat.Light")]
    public class MainActivity
		:
		AppCompatActivity,
		IPaymentMethodNonceCreatedListener, 
		IBraintreeCancelListener, 
		IBraintreeErrorListener, 
		DropInResult.IDropInResultListener
    {
		static string KEY_AUTHORIZATION = "com.braintreepayments.demo.KEY_AUTHORIZATION";
		static int DROP_IN_REQUEST = 100;

        static string KEY_NONCE = "nonce";

		protected string mAuthorization;
		protected string mCustomerId;
		protected BraintreeFragment mBraintreeFragment;

		PaymentMethodType mPaymentMethodType;
        PaymentMethodNonce mNonce;

        CardView mPaymentMethod;
        ImageView mPaymentMethodIcon;
        TextView mPaymentMethodTitle;
        TextView mPaymentMethodDescription;
        TextView mNonceString;
        TextView mNonceDetails;
        TextView mDeviceData;

        Button mAddPaymentMethodButton;
        Button mPurchaseButton;
        ProgressDialog mLoading;

        bool mShouldMakePurchase = false;

        bool mPurchased = false;

		internal static MainActivity Instance { get; private set; }

		protected override void OnCreate(Bundle savedInstanceState)
        {
			Instance = this;

			base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.main_activity);

            mPaymentMethod = FindViewById<CardView>(Resource.Id.payment_method);
            mPaymentMethodIcon = FindViewById<ImageView>(Resource.Id.payment_method_icon);
            mPaymentMethodTitle = FindViewById<TextView>(Resource.Id.payment_method_title);
            mPaymentMethodDescription = FindViewById<TextView>(Resource.Id.payment_method_description);
            mNonceString = FindViewById<TextView>(Resource.Id.nonce);
            mNonceDetails = FindViewById<TextView>(Resource.Id.nonce_details);
            mDeviceData = FindViewById<TextView>(Resource.Id.device_data);

            mAddPaymentMethodButton = FindViewById<Button>(Resource.Id.add_payment_method);
            mAddPaymentMethodButton.Click += MAddPaymentMethodButton_Click;

            mPurchaseButton = FindViewById<Button>(Resource.Id.purchase);

			mCustomerId = "test_1299654099_biz_api1.kevinchows.com";

			if (savedInstanceState != null)
            {
                if (savedInstanceState.ContainsKey(KEY_NONCE))
                {
                    mNonce = (PaymentMethodNonce)savedInstanceState.GetParcelable(KEY_NONCE);
                }
            }
        }

        void MAddPaymentMethodButton_Click(object sender, System.EventArgs e)
        {
			PaymentService.SubmitPayment(mAuthorization);
			//AddPaymentMethod();
        }

		public void AddPaymentMethod()
		{
			DropInRequest dropInRequest = new DropInRequest();
			dropInRequest.ClientToken(mAuthorization);
			dropInRequest.Amount("1.00");

			StartActivityForResult(dropInRequest.GetIntent(this), DROP_IN_REQUEST);
		}

		protected override void OnResume()
        {
			PayPalOneTouchCore.UseHardcodedConfig(this, true);
			mAuthorization = "sandbox_tmxhyf7d_dcpspy2brwdjr3qn";

			base.OnResume();

            if (mPurchased)
            {
                mPurchased = false;
                clearNonce();

                try
                {
                    if (ClientToken.FromString(mAuthorization) is ClientToken)
                    {
                        DropInResult.FetchDropInResult(this, mAuthorization, this);
                    }
                    else
                    {
                        mAddPaymentMethodButton.Visibility = Android.Views.ViewStates.Visible;
                    }
                }
                catch (InvalidArgumentException e)
                {
                    mAddPaymentMethodButton.Visibility = Android.Views.ViewStates.Visible;
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
			if (mAuthorization != null)
			{
				outState.PutString(KEY_AUTHORIZATION, mAuthorization);
			}
			if (mNonce != null)
            {
                outState.PutParcelable(KEY_NONCE, mNonce);
            }
        }
        public void OnPaymentMethodNonceCreated(PaymentMethodNonce paymentMethodNonce)
        {
            System.Diagnostics.Debug.WriteLine("Payment Method Nonce received: " + paymentMethodNonce.TypeLabel);
        }

        public void OnCancel(int requestCode)
        {
            System.Diagnostics.Debug.WriteLine("Cancel received: " + requestCode);
        }

        public void OnError(Java.Lang.Exception error)
        {
            System.Diagnostics.Debug.WriteLine("Error received (" + error.GetType() + "): " + error.Message);
            //mLogger.debug(error.toString());
        }

        [ExportAttribute("purchase")]
        public void Purchase(View v)
        {
            if (mPaymentMethodType == PaymentMethodType.AndroidPay && mNonce == null)
            {
                List<Android.Gms.Identity.Intents.Model.CountrySpecification> countries = new List<Android.Gms.Identity.Intents.Model.CountrySpecification>();

                mShouldMakePurchase = true;
            }
            else
            {
                Intent intent = new Intent(this, typeof(CreateTransactionActivity))
                    .PutExtra("nonce", mNonce);
                StartActivity(intent);

                mPurchased = true;
            }
        }


        [ExportAttribute("launchDropIn")]
        public void launchDropIn(View v)
        {
            MAddPaymentMethodButton_Click(v, EventArgs.Empty);
        }

        public void OnResult(DropInResult result)
        {
            if (result.PaymentMethodType == null)
            {
                mAddPaymentMethodButton.Visibility = Android.Views.ViewStates.Visible;
            }
            else
            {
                mAddPaymentMethodButton.Visibility = Android.Views.ViewStates.Gone;

                mPaymentMethodType = result.PaymentMethodType;

                mPaymentMethodIcon.SetImageResource(result.PaymentMethodType.Drawable);
                if (result.PaymentMethodNonce != null)
                {
                    DisplayResult(result.PaymentMethodNonce, result.DeviceData);
                }
                else if (result.PaymentMethodType == PaymentMethodType.AndroidPay)
                {
                    mPaymentMethodTitle.SetText(PaymentMethodType.AndroidPay.LocalizedName);
                    mPaymentMethodDescription.Text = "";
                    mPaymentMethod.Visibility = Android.Views.ViewStates.Visible;
                }

                mPurchaseButton.Enabled = true;
            }

        }

        private void clearNonce()
        {
            mPaymentMethod.Visibility = Android.Views.ViewStates.Gone;
            mNonceString.Visibility = Android.Views.ViewStates.Gone;
            mNonceDetails.Visibility = Android.Views.ViewStates.Gone;
            mDeviceData.Visibility = Android.Views.ViewStates.Gone;
        }

        private string formatAddress(PostalAddress address)
        {
            return address.RecipientName + " " + address.StreetAddress + " " +
                address.ExtendedAddress + " " + address.Locality + " " + address.Region +
                    " " + address.PostalCode + " " + address.CountryCodeAlpha2;
        }

        private string formatAddress(UserAddress address)
        {
            if (address == null)
            {
                return "null";
            }
            return address.Name + " " + address.Address1 + " " + address.Address2 + " " +
                    address.Address3 + " " + address.Address4 + " " + address.Address5 + " " +
                    address.Locality + " " + address.AdministrativeArea + " " + address.PostalCode + " " +
                    address.SortingCode + " " + address.CountryCode;
        }
        private Cart getAndroidPayCart()
        {
            return Cart.NewBuilder()
                    .SetCurrencyCode("USD")
                    .SetTotalPrice("1.00")
                    .AddLineItem(LineItem.NewBuilder()
                            .SetCurrencyCode("USD")
                            .SetDescription("Description")
                            .SetQuantity("1")
                            .SetUnitPrice("1.00")
                            .SetTotalPrice("1.00")
                            .Build())
                    .Build();
        }

        private void DisplayResult(PaymentMethodNonce paymentMethodNonce, string deviceData)
        {
            mNonce = paymentMethodNonce;
            mPaymentMethodType = PaymentMethodType.ForType(mNonce);

            mPaymentMethodIcon.SetImageResource(PaymentMethodType.ForType(mNonce).Drawable);
            mPaymentMethodTitle.Text = paymentMethodNonce.TypeLabel;
            mPaymentMethodDescription.Text = paymentMethodNonce.Description;
            mPaymentMethod.Visibility = Android.Views.ViewStates.Visible;

            mNonceString.Text = GetString(Resource.String.nonce) + ": " + mNonce.Nonce;
            mNonceString.Visibility = Android.Views.ViewStates.Visible;

            string details = "";
            if (mNonce is CardNonce)
            {
                CardNonce cardNonce = (CardNonce)mNonce;

                details = "Card Last Two: " + cardNonce.LastTwo + "\n";
                details += "3DS isLiabilityShifted: " + cardNonce.ThreeDSecureInfo.IsLiabilityShifted + "\n";
                details += "3DS isLiabilityShiftPossible: " + cardNonce.ThreeDSecureInfo.IsLiabilityShiftPossible;
            }
            else if (mNonce is PayPalAccountNonce)
            {
                PayPalAccountNonce paypalAccountNonce = (PayPalAccountNonce)mNonce;

                details = "First name: " + paypalAccountNonce.FirstName + "\n";
                details += "Last name: " + paypalAccountNonce.LastName + "\n";
                details += "Email: " + paypalAccountNonce.Email + "\n";
                details += "Phone: " + paypalAccountNonce.Phone + "\n";
                details += "Payer id: " + paypalAccountNonce.PayerId + "\n";
                details += "Client metadata id: " + paypalAccountNonce.ClientMetadataId + "\n";
                details += "Billing address: " + formatAddress(paypalAccountNonce.BillingAddress) + "\n";
                details += "Shipping address: " + formatAddress(paypalAccountNonce.ShippingAddress);
            }
            else if (mNonce is AndroidPayCardNonce)
            {
                AndroidPayCardNonce androidPayCardNonce = (AndroidPayCardNonce)mNonce;

                details = "Underlying Card Last Two: " + androidPayCardNonce.LastTwo + "\n";
                details += "Email: " + androidPayCardNonce.Email + "\n";
                details += "Billing address: " + formatAddress(androidPayCardNonce.BillingAddress) + "\n";
                details += "Shipping address: " + formatAddress(androidPayCardNonce.ShippingAddress);
            }
            else if (mNonce is VenmoAccountNonce)
            {
                VenmoAccountNonce venmoAccountNonce = (VenmoAccountNonce)mNonce;

                details = "Username: " + venmoAccountNonce.Username;
            }

            mNonceDetails.Text = details;
            mNonceDetails.Visibility = Android.Views.ViewStates.Visible;

            mDeviceData.Text = "Device Data: " + deviceData;
            mDeviceData.Visibility = Android.Views.ViewStates.Visible;

            mAddPaymentMethodButton.Visibility = Android.Views.ViewStates.Gone;
            mPurchaseButton.Enabled = true;
        }

        void SafelyCloseLoadingView()
        {
            if (mLoading != null && mLoading.IsShowing)
            {
                mLoading.Dismiss();
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            SafelyCloseLoadingView();

            if (resultCode == Android.App.Result.Ok)
            {
                DropInResult result = (DropInResult)data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                DisplayResult(result.PaymentMethodNonce, result.DeviceData);
                mPurchaseButton.Enabled = (true);
            }
            else if (resultCode != Android.App.Result.Canceled)
            {
                SafelyCloseLoadingView();
                var error = data.GetSerializableExtra(DropInActivity.ExtraError);

				Java.Lang.Exception exeption = (Java.Lang.Exception)error;

				System.Diagnostics.Debug.WriteLine((exeption).Message);
			}
		}
	}
}

