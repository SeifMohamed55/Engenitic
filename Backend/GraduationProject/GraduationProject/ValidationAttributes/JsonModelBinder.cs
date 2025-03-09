namespace GraduationProject.ValidationAttributes
{
    using System.Text;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json;

    public class JsonModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid JSON input.");
                return Task.CompletedTask;
            }

            try
            {
                var result = JsonConvert.DeserializeObject(value, bindingContext.ModelType);
                bindingContext.Result = ModelBindingResult.Success(result);
            }
            catch (JsonException)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Invalid JSON format.");
            }

            return Task.CompletedTask;
        }
    }

}
