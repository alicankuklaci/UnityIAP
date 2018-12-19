using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Linq;

namespace KUKLACI
{


    public class MyProduct {
        public string ProductName { get; set; }
        public ProductType ProductType { get; set; }

    }
    // Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
    public class Purchaser : MonoBehaviour, IStoreListener
    {
        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private List<MyProduct> MyProductList;
   



        // declare delegates 
        public delegate void MyOnPurchaseFailed(Product product, PurchaseFailureReason failureReason);
        public delegate void MyOnPurchaseProcessingResult(PurchaseProcessingResult result,MyProduct product);
        public delegate void MyOnAllError(String Message);
        public delegate void MyOnInitialized();

        //declare event of type delegate
        public event MyOnPurchaseFailed MyOnPurchaseFailed_;
        public event MyOnPurchaseProcessingResult MyOnPurchaseProcessingResult_;
        public event MyOnAllError MyOnAllError_;
        public event MyOnInitialized MyOnInitialized_;






        public Purchaser(List<MyProduct> _MyProducts)
        {
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null)
            {

                MyProductList = _MyProducts;
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
               

            }

        }

        void Start()
        {
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController == null)
            {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
        }


       


        public void InitializePurchasing()
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {

                if (MyOnAllError_ != null)
                    MyOnAllError_("Already initialized");
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add a product to list
    
            MyProductList.ForEach(x => {
                builder.AddProduct(x.ProductName, x.ProductType);
            });

      

            UnityPurchasing.Initialize(this, builder);

            if (MyOnInitialized_ != null)
                MyOnInitialized_();
        }


        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }


        public void BuyProduct(MyProduct p)
        {
            // Buy a product using its general identifier. Expect a response either 
            // through ProcessPurchase or OnPurchaseFailed asynchronously.
            BuyProductID(p.ProductName);
        }





        void BuyProductID(string productId)
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    if (MyOnAllError_ != null)
                        MyOnAllError_("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initiailization.
                if (MyOnAllError_ != null)
                    MyOnAllError_("BuyProductID FAIL. Not initialized.");

                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }


        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                if (MyOnAllError_ != null)
                    MyOnAllError_("BuyProductID FAIL. Not initialized.");
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) => {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
                if (MyOnAllError_ != null)
                    MyOnAllError_("RestorePurchases FAIL. Not supported on this platform. Current = \" + Application.platform");
            }
        }


        //  
        // --- IStoreListener
        //

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);

            if (MyOnAllError_ != null)
                MyOnAllError_("OnInitializeFailed InitializationFailureReason: " + error);

        }


        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {


            var isHaveProduct = MyProductList.Where(x => x.ProductName == args.purchasedProduct.definition.id).FirstOrDefault();

            if (isHaveProduct!=null)
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            }

            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            else
            {
                if (MyOnAllError_ != null)
                    MyOnAllError_("ProcessPurchase: FAIL. Unrecognized product: "+ args.purchasedProduct.definition.id);

                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            if (MyOnPurchaseProcessingResult_ != null)
                MyOnPurchaseProcessingResult_(PurchaseProcessingResult.Complete, isHaveProduct);

            return PurchaseProcessingResult.Complete;
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
            if (MyOnPurchaseFailed_ != null)
                MyOnPurchaseFailed_(product,failureReason);
        }
    }
}
