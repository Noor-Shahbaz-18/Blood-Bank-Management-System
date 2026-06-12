using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class NavigationHelper
    {
        // Stack to track form navigation history
        private static Stack<Form> _navigationStack = new Stack<Form>();

        // Current active form
        private static Form _currentForm;

        // Navigate to new form
        public static void NavigateTo(Form currentForm, Form newForm)
        {
            _currentForm = currentForm;
            _navigationStack.Push(currentForm);
            currentForm.Hide();
            newForm.FormClosed += (s, e) => GoBack();
            newForm.Show();
        }

        // Go back to previous form
        public static void GoBack()
        {
            if (_navigationStack.Count > 0)
            {
                Form previousForm = _navigationStack.Pop();
                previousForm.Show();
            }
        }

        // Clear navigation history
        public static void ClearHistory()
        {
            _navigationStack.Clear();
        }
    }
}