using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BillsSystem.Web.Extensions
{
    public static class ModelStateExtensions
    {
        // بترجّع true بس لو فيه قيمة مكتوبة فعلاً وفشل تحويلها (مثلاً "abc" في حقل رقمي).
        // الحقول الفاضية بتتشال من الـ ModelState وبتروح لـ FluentValidation عشان يدّي رسالة الـ SRS.
        public static bool HasRealBindingErrors(this ModelStateDictionary state)
        {
            var emptyKeys = new List<string>();
            var hasRealErrors = false;

            foreach (var (key, entry) in state)
            {
                if (entry.ValidationState != ModelValidationState.Invalid) continue;

                if (string.IsNullOrWhiteSpace(entry.AttemptedValue))
                    emptyKeys.Add(key);
                else
                    hasRealErrors = true;
            }

            foreach (var key in emptyKeys)
                state.ClearValidationState(key);

            return hasRealErrors;
        }
    }
}
