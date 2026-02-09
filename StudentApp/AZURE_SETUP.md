# Azure AD Configuration Update Required

The error `AADSTS50011` occurs because we changed the App's **Bundle ID** to fix the iOS provisioning, but the **Azure Portal** still expects the old ID.

## 🛑 Action Required: Update Azure Portal

1.  Go to the [Azure Portal](https://portal.azure.com/).
2.  Navigate to **Microsoft Entra ID** (formerly Azure AD) -> **App registrations**.
3.  Find and select your app: `StudentApp` (Client ID: `5272e248-fbdb-4761-888a-77c8b1f91ae6`).
4.  Go to **Authentication** in the left menu.
5.  Look for the **iOS/macOS** platform configuration.
6.  **Add a new Redirect URI**:
    ```text
    msauth.com.carlos.kiosko.studentapp://auth
    ```
    *(Note: it must match exactly what is in the error message)*
7.  **Save** the changes.

## 📱 After Updating
1.  Wait a minute for the changes to propagate.
2.  Close the app on the simulator/device.
3.  Run the app again and try to log in.
