# UnityIAP
You can integrate Unity IAP functions with this script quickly

# Events
```cs
        public delegate void MyOnPurchaseFailed(Product product, PurchaseFailureReason failureReason);
        public delegate void MyOnPurchaseProcessingResult(PurchaseProcessingResult result,MyProduct product);
        public delegate void MyOnAllError(String Message);
        public delegate void MyOnInitialized();
```

# Usage

Add this script to your project.

### NOTE : Namespaces must be same.

## Declare global variable first
```cs
        private Purchaser inapp;
        private List<MyProduct> products;
 ```
 
 ## Prepare your code to catch IAP functions
 ```cs
 	void Start () {

            products = new List<MyProduct>();
            products.Add(new MyProduct {ProductName= "com.alican.duckhunter.para0001", ProductType= ProductType.Consumable });
            products.Add(new MyProduct { ProductName = "com.alican.duckhunter.para0002", ProductType = ProductType.Consumable });
            products.Add(new MyProduct { ProductName = "com.alican.duckhunter.para0003", ProductType = ProductType.Consumable });


            inapp = new KUKLACI.Purchaser(products);


            inapp.MyOnAllError_+=Inapp_MyOnAllError_;
            inapp.MyOnInitialized_+=Inapp_MyOnInitialized_;
            inapp.MyOnPurchaseFailed_+=Inapp_MyOnPurchaseFailed_;
            inapp.MyOnPurchaseProcessingResult_+=Inapp_MyOnPurchaseProcessingResult_;

}
 ```
 
 ## And then, you able to ready to use
  ```cs
  

        private void Inapp_MyOnPurchaseProcessingResult_(PurchaseProcessingResult result,MyProduct product)
        {
            if (result==PurchaseProcessingResult.Complete)
            {
                Debug.Log("purchase success");
                
         
            }
        }

        private void Inapp_MyOnPurchaseFailed_(Product product, PurchaseFailureReason failureReason)
        {
                 
            Debug.Log("failed purchase");
        }

        private void Inapp_MyOnInitialized_()
        {
            Debug.Log("initialized purchase");
        }

        private void Inapp_MyOnAllError_(string Message)
        {
            Debug.Log("Error "+ Message);
        }
   ```
 
 
 
