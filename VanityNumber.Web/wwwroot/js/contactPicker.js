// Contact Picker API wrapper for Blazor
window.contactPicker = {
    // Check if the Contact Picker API is supported
    isSupported: function () {
        return 'contacts' in navigator && 'ContactsManager' in window;
    },

    // Open the contact picker and return selected phone number
    pickContact: async function () {
        try {
            if (!this.isSupported()) {
                throw new Error('Contact Picker API is not supported in this browser');
            }

            const props = ['tel'];
            const opts = { multiple: false };

            const contacts = await navigator.contacts.select(props, opts);

            if (contacts && contacts.length > 0) {
                const contact = contacts[0];
                
                // Get the first phone number from the contact
                if (contact.tel && contact.tel.length > 0) {
                    return contact.tel[0];
                }
            }

            return null;
        } catch (error) {
            console.error('Error picking contact:', error);
            throw error;
        }
    }
};
