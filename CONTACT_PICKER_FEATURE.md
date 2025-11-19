# Contact Picker Feature

## Overview
The Contact Picker feature allows users on supported mobile browsers to select phone numbers directly from their device's address book, making it easier to generate vanity numbers for their contacts.

## Browser Support

### ✅ Supported Browsers
- **Chrome/Edge on Android** (version 80+)
  - Full support for Contact Picker API
  - Native contact selection dialog
  - Automatic permission handling

### ❌ Not Supported
- **iOS Safari** - Apple does not support the Contact Picker API
- **Desktop browsers** - API is mobile-only
- **Firefox Mobile** - Limited/no support

## How It Works

### User Experience
1. When the page loads, the system checks if the Contact Picker API is available
2. If supported, a "📇 Contacts" button appears next to the phone number input
3. User taps the button to open their native contacts picker
4. User selects a contact with a phone number
5. The phone number is automatically populated in the input field
6. User can then generate vanity numbers as normal

### Technical Implementation

#### JavaScript API (`contactPicker.js`)
```javascript
window.contactPicker = {
    isSupported: function () {
        return 'contacts' in navigator && 'ContactsManager' in window;
    },
    
    pickContact: async function () {
        const contacts = await navigator.contacts.select(['tel'], { multiple: false });
        return contacts[0]?.tel[0] || null;
    }
};
```

#### Blazor Integration
- `OnAfterRenderAsync`: Checks API support on first render
- `SelectFromContacts()`: Invokes JavaScript to open contact picker
- Conditional rendering: Button only shows when API is supported

## Privacy & Permissions

### User Control
- **Explicit permission**: User must explicitly select a contact
- **One-time access**: Each selection requires user interaction
- **No background access**: Cannot access contacts without user gesture
- **No contact list**: App never receives the full contact list

### Security
- Requires HTTPS (secure connection)
- User gesture required (button click)
- Browser-controlled permissions
- No persistent contact access

## Error Handling

The feature includes graceful error handling:
- API not supported → Button doesn't appear
- User cancels selection → No action taken
- Permission denied → Error message displayed
- Network errors → Caught and logged

## Testing

### On Android Chrome
1. Open the app on Android device
2. Verify the "Contacts" button appears
3. Tap the button
4. Select a contact with a phone number
5. Verify number populates in the input field

### On Unsupported Browsers
1. Open the app on iOS or desktop
2. Verify the "Contacts" button does NOT appear
3. User can still manually enter phone numbers

## Future Enhancements

Potential improvements:
1. **Contact name display**: Show selected contact's name
2. **Multiple selections**: Allow selecting multiple contacts
3. **Recent contacts**: Show recently used numbers
4. **Polyfill for iOS**: Research alternative methods for iOS

## Code Files

- **JavaScript**: `VanityNumber.Web/wwwroot/js/contactPicker.js`
- **Blazor Component**: `VanityNumber.Web/Pages/Home.razor`
- **Styles**: `VanityNumber.Web/wwwroot/css/app.css`
- **HTML**: `VanityNumber.Web/wwwroot/index.html`

## Resources

- [MDN: Contact Picker API](https://developer.mozilla.org/en-US/docs/Web/API/Contact_Picker_API)
- [Web.dev: Contact Picker](https://web.dev/contact-picker/)
- [Can I Use: Contact Picker API](https://caniuse.com/contact-picker)
