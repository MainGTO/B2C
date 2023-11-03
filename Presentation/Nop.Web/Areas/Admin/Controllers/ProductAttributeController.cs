using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using DeepL;
using static Nop.Web.Areas.Admin.Controllers.ProductController;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductAttributeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeModelFactory _productAttributeModelFactory;
        private readonly IProductAttributeService _productAttributeService;

        #endregion Fields

        #region Ctor

        public ProductAttributeController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductAttributeModelFactory productAttributeModelFactory,
            IProductAttributeService productAttributeService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productAttributeModelFactory = productAttributeModelFactory;
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Translate DeppL

        public async Task<string> DeppLTranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            var authKey = "67d0450a-748d-4bf2-d0ba-cf5dad9fa30f:fx";  // DeepL의 Auth Key
            System.Diagnostics.Debug.WriteLine($"Translating text: '{text}' from {sourceLanguage} to {targetLanguage}");

            var translator = new Translator(authKey);

            // sourceLanguage가 null이거나 빈 문자열일 경우 null을 전달
            var actualSourceLanguage = string.IsNullOrWhiteSpace(sourceLanguage) ? null : sourceLanguage;
            var translatedText = await translator.TranslateTextAsync(text, actualSourceLanguage, targetLanguage);

            System.Diagnostics.Debug.WriteLine($"Translated text: '{translatedText.Text}'");
            return translatedText.Text;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(ProductAttribute productAttribute, ProductAttributeModel model, bool api = false)
        {
            model.Locales = model.Locales.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();
            //var existingLanguagesWithShortDescription = model.Locales.Where(l => !string.IsNullOrEmpty(l.ShortDescription)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r)
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, productAttribute.Id, "Product", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행
                foreach (var langCode in allLanguages.Keys)
                {
                    if (langCode == "ko")
                    {
                        translations[langCode] = model.Name; // 한국어를 한국어로 번역하는 대신 기존 값을 사용
                    }
                    else if (int.TryParse(model.Name, out _))
                    {
                        translations[langCode] = model.Name; // model.Name이 숫자로만 구성되어 있다면 그대로 사용
                    }
                    else
                    {
                        translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                    }
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductAttributeLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(PredefinedProductAttributeValue ppav, PredefinedProductAttributeValueModel model, bool api = false)
        {
            model.Locales = model.Locales.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();
            //var existingLanguagesWithShortDescription = model.Locales.Where(l => !string.IsNullOrEmpty(l.ShortDescription)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r)
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, ppav.Id, "Product", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행
                foreach (var langCode in allLanguages.Keys)
                {
                    if (langCode == "ko")
                    {
                        translations[langCode] = model.Name; // 한국어를 한국어로 번역하는 대신 기존 값을 사용
                    }
                    else if (int.TryParse(model.Name, out _))
                    {
                        translations[langCode] = model.Name; // model.Name이 숫자로만 구성되어 있다면 그대로 사용
                    }
                    else
                    {
                        translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                    }
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new PredefinedProductAttributeValueLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(ppav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        #endregion

        private static string ConvertToLanguageCode(string languageId)
        {
            switch (languageId)
            {
                case "1":
                    return "en-US";
                case "2":
                    return "ko";
                case "3":
                    return "zh";
                default:
                    return languageId;
            }
        }

        #region Methods

        #region Attribute list / create / edit / delete

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //prepare model
            var model = await _productAttributeModelFactory.PrepareProductAttributeSearchModelAsync(new ProductAttributeSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProductAttributeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productAttributeModelFactory.PrepareProductAttributeListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //prepare model
            var model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(new ProductAttributeModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();
                        
            if (ModelState.IsValid)
            {
                var productAttribute = model.ToEntity<ProductAttribute>();
                await _productAttributeService.InsertProductAttributeAsync(productAttribute);
                await UpdateLocalesAsync(productAttribute, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewProductAttribute",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProductAttribute"), productAttribute.Name), productAttribute);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = productAttribute.Id });
            }

            //prepare model
            model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);
            if (productAttribute == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(null, productAttribute);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            model.Name = await DeppLTranslateTextAsync(model.Name, "", LanguageCode.Korean);

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.Id);
            if (productAttribute == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productAttribute = model.ToEntity(productAttribute);
                await _productAttributeService.UpdateProductAttributeAsync(productAttribute);

                await UpdateLocalesAsync(productAttribute, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditProductAttribute",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProductAttribute"), productAttribute.Name), productAttribute);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");
                
                return RedirectToAction("Edit", new { id = productAttribute.Id });
            }

            //prepare model
            model = await _productAttributeModelFactory.PrepareProductAttributeModelAsync(model, productAttribute, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(id);
            if (productAttribute == null)
                return RedirectToAction("List");

            await _productAttributeService.DeleteProductAttributeAsync(productAttribute);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteProductAttribute",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProductAttribute"), productAttribute.Name), productAttribute);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var productAttributes = await _productAttributeService.GetProductAttributeByIdsAsync(selectedIds.ToArray());
            await _productAttributeService.DeleteProductAttributesAsync(productAttributes);

            foreach (var productAttribute in productAttributes)
            {
                await _customerActivityService.InsertActivityAsync("DeleteProductAttribute",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProductAttribute"), productAttribute.Name), productAttribute);
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Used by products

        [HttpPost]
        public virtual async Task<IActionResult> UsedByProducts(ProductAttributeProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return await AccessDeniedDataTablesJson();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(searchModel.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            //prepare model
            var model = await _productAttributeModelFactory.PrepareProductAttributeProductListModelAsync(searchModel, productAttribute);

            return Json(model);
        }

        #endregion

        #region Predefined values

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedProductAttributeValueList(PredefinedProductAttributeValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return await AccessDeniedDataTablesJson();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(searchModel.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            //prepare model
            var model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueListModelAsync(searchModel, productAttribute);

            return Json(model);
        }

        public virtual async Task<IActionResult> PredefinedProductAttributeValueCreatePopup(int productAttributeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id", nameof(productAttributeId));

            //prepare model
            var model = await _productAttributeModelFactory
                .PreparePredefinedProductAttributeValueModelAsync(new PredefinedProductAttributeValueModel(), productAttribute, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedProductAttributeValueCreatePopup(PredefinedProductAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(model.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            if (ModelState.IsValid)
            {
                //fill entity from model
                var ppav = model.ToEntity<PredefinedProductAttributeValue>();

                await _productAttributeService.InsertPredefinedProductAttributeValueAsync(ppav);
                await UpdateLocalesAsync(ppav, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(model, productAttribute, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> PredefinedProductAttributeValueEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a predefined product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No predefined product attribute value found with the specified id");

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeValue.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            //prepare model
            var model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(null, productAttribute, productAttributeValue);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedProductAttributeValueEditPopup(PredefinedProductAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a predefined product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(model.Id)
                ?? throw new ArgumentException("No predefined product attribute value found with the specified id");

            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeValue.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            if (ModelState.IsValid)
            {
                productAttributeValue = model.ToEntity(productAttributeValue);
                await _productAttributeService.UpdatePredefinedProductAttributeValueAsync(productAttributeValue);

                await UpdateLocalesAsync(productAttributeValue, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productAttributeModelFactory.PreparePredefinedProductAttributeValueModelAsync(model, productAttribute, productAttributeValue, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PredefinedProductAttributeValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            //try to get a predefined product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetPredefinedProductAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No predefined product attribute value found with the specified id", nameof(id));

            await _productAttributeService.DeletePredefinedProductAttributeValueAsync(productAttributeValue);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}