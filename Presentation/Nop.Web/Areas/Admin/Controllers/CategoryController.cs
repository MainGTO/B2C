using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using DeepL;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CategoryController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public CategoryController(IAclService aclService,
            ICategoryModelFactory categoryModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IExportManager exportManager,
            IImportManager importManager,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IStaticCacheManager staticCacheManager,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            System.Net.Http.IHttpClientFactory httpClientFactory,
            ILogger logger)
        {
            _aclService = aclService;
            _categoryModelFactory = categoryModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #endregion

        #region Translate DeppL

        public async Task<string> DeppLTranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            System.Diagnostics.Debug.WriteLine($"Translating text: '{text}' from {sourceLanguage} to {targetLanguage}");

            var authKey = "f2fc50f9-4601-cfc5-9eec-576e8a73cf41:fx";  // DeepL의 Auth Key
            var translator = new Translator(authKey);
            var translatedText = await translator.TranslateTextAsync(
                  text,
                  sourceLanguage,
                  targetLanguage);

            System.Diagnostics.Debug.WriteLine($"Translated text: '{translatedText.Text}'");
            return translatedText.Text;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateLocalesAsync(Category category, CategoryModel model , bool api = false)
        {
            // model.Name이 숫자로만 구성되어 있다면 메서드 종료
            if (int.TryParse(model.Name, out _))
                return;

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
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, category.Id, "Product", "Name");
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
                        model.Locales.Add(new CategoryLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(category,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(category, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(category, seName, localized.LanguageId);
            }
        }

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

        protected virtual async Task UpdatePictureSeoNamesAsync(Category category)
        {
            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            if (picture != null)
                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(category.Name));
        }

        protected virtual async Task SaveCategoryAclAsync(Category category, CategoryModel model)
        {
            category.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _categoryService.UpdateCategoryAsync(category);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(category);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(category, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task SaveStoreMappingsAsync(Category category, CategoryModel model)
        {
            category.LimitedToStores = model.SelectedStoreIds.Any();
            await _categoryService.UpdateCategoryAsync(category);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(category);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(category, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete / Translate

        // Google Translate API 대상 텍스트 언어 감지
        public async Task<string> DetectLanguageAsync(string text)
        {
            try
            {
                var apiKey = "AIzaSyD4CI-ZD19kRHdzp-8Ag9hC_sEdNc6JZnY";  // 추후에 환경 변수나 구성 파일에서 읽어오는 것으로 구현
                var url = $"https://translation.googleapis.com/language/translate/v2/detect?key={apiKey}&q={text}";

                using (HttpClient client = new HttpClient())
                {
                    System.Diagnostics.Debug.WriteLine($"Detecting language for text: {text}");

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    var detectedLanguage = jsonObject["data"]["detections"][0][0]["language"].ToString();

                    // 만약 감지된 언어가 "zh-"로 시작하면 "zh-CN"으로 강제 설정
                    if (detectedLanguage.StartsWith("zh-"))
                    {
                        detectedLanguage = "zh-CN";
                    }
                    // 만약 감지된 언어가 "ar-Latn"이면 "ar"로 강제 설정
                    else if (detectedLanguage == "ar-Latn")
                    {
                        detectedLanguage = "ar";
                    }

                    return detectedLanguage;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Google Translate API 호출 중 오류 발생: {ex.Message}");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"JSON 파싱 중 오류 발생: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"서버 내부 오류: {ex.Message}");
            }
        }

        private Dictionary<string, string> _translationCache = new Dictionary<string, string>();

        // Google Translate API 번역 함수
        public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            // 지원하는 언어 목록
            var supportedLanguages = new HashSet<string>
            {
                "af", "sq", "am", "ar", "hy", "as", "ay", "az", "bm", "eu", "be", "bn",
                "bho", "bs", "bg", "ca", "ceb", "zh-CN", "zh-TW", "co", "hr", "cs", "da",
                "dv", "doi", "nl", "en", "eo", "et", "ee", "fil", "fi", "fr", "fy", "gl",
                "ka", "de", "el", "gn", "gu", "ht", "ha", "haw", "he", "hi", "hmn", "hu",
                "is", "ig", "ilo", "id", "ga", "it", "ja", "jv", "kn", "kk", "km", "rw",
                "gom", "ko", "kri", "ku", "ckb", "ky", "lo", "la", "lv", "ln", "lt", "lg",
                "lb", "mk", "mai", "mg", "ms", "ml", "mt", "mi", "mr", "mni-Mtei", "lus",
                "mn", "my", "ne", "no", "ny", "or", "om", "ps", "fa", "pl", "pt", "pa",
                "qu", "ro", "ru", "sm", "sa", "gd", "nso", "sr", "st", "sn", "sd", "si",
                "sk", "sl", "so", "es", "su", "sw", "sv", "tl", "tg", "ta", "tt", "te",
                "th", "ti", "ts", "tr", "tk", "ak", "uk", "ur", "ug", "uz", "vi", "cy",
                "xh", "yi", "yo", "zu"
            };

            // BCP-47 태그에서 기본 언어 코드만 추출
            string convertSpecialLanguageCodes(string langTag)
            {
                // 특정 언어 태그에 대해 특별한 처리
                switch (langTag)
                {
                    case "zh-CN":
                        return "zh-CN";
                    case "zh-TW":
                        return "zh-TW";
                    default:
                        return langTag.Split('-')[0];
                }
            }

            var primarySourceLanguage = convertSpecialLanguageCodes(sourceLanguage);
            var primaryTargetLanguage = convertSpecialLanguageCodes(targetLanguage);

            // sourceLanguage와 targetLanguage가 지원하는 언어인지 확인
            if (!supportedLanguages.Contains(primarySourceLanguage) || !supportedLanguages.Contains(primaryTargetLanguage))
            {
                var errorMessage = $"Translating text: {text} from {sourceLanguage} to {targetLanguage} [에러(ERROR)]";
                return $"{text} {errorMessage}";
            }

            // 캐시에서 번역된 텍스트 확인
            var cacheKey = $"{sourceLanguage}-{targetLanguage}:{text}";
            if (_translationCache.ContainsKey(cacheKey))
            {
                return _translationCache[cacheKey];
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"Translating text: {text} from {sourceLanguage} to {targetLanguage}");

                var apiKey = "AIzaSyD4CI-ZD19kRHdzp-8Ag9hC_sEdNc6JZnY";  // 추후에 환경 변수나 구성 파일에서 읽어오는 것으로 구현
                var encodedText = HttpUtility.UrlEncode(text);
                var url = $"https://translation.googleapis.com/language/translate/v2?source={sourceLanguage}&target={targetLanguage}&key={apiKey}&q={encodedText}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    // "translatedText" 값을 안전하게 추출
                    var translatedText = jsonObject["data"]?["translations"]?[0]?["translatedText"]?.ToString();

                    if (translatedText == null)
                    {
                        var errorMessage = $"Google Translate API에서 예상치 못한 응답을 받았습니다. Translating text: {text} from {sourceLanguage} to {targetLanguage} [에러(ERROR)]";
                        return $"{text} {errorMessage}";
                    }

                    // 번역된 텍스트를 캐시에 저장
                    _translationCache[cacheKey] = translatedText;

                    return translatedText;
                }
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = $"Google Translate API 호출 중 오류 발생: {ex.Message}. Translating text: {text} from {sourceLanguage} to {targetLanguage} [에러(ERROR)]";
                return $"{text} {errorMessage}";
            }
            catch (JsonReaderException ex)
            {
                var errorMessage = $"JSON 파싱 중 오류 발생: {ex.Message}. Translating text: {text} from {sourceLanguage} to {targetLanguage} [에러(ERROR)]";
                return $"{text} {errorMessage}";
            }
            catch (Exception ex)
            {
                var errorMessage = $"서버 내부 오류: {ex.Message}. Translating text: {text} from {sourceLanguage} to {targetLanguage} [에러(ERROR)]";
                return $"{text} {errorMessage}";
            }
        }

        private readonly Dictionary<int, string> _targetLanguages = new Dictionary<int, string>
        {
            { 1, "en" },
            { 2, "ko" },
            { 3, "zh-CN" },
            // { 4, "vi" },
            // { 5, "th" },
            // 추가 언어 가능
        };

        // Google Translate API Main
        public async Task<List<TResult>> TranslateAndFillAsync<TModel, TResult>(TModel model)
                       where TModel : class
                       where TResult : class, new()
        {
            System.Diagnostics.Debug.WriteLine("Starting translation process for provided model.");

            var localizedModels = new List<TResult>();
            var baseProperties = typeof(TModel).GetProperties().Where(p => p.PropertyType == typeof(string) && p.Name != "Description" && p.Name != "PageSizeOptions");

            // 기준 언어 감지 (Name 속성을 기준으로 함)
            var baseText = typeof(TModel).GetProperty("Name").GetValue(model).ToString();
            var baseLanguage = await DetectLanguageAsync(baseText);

            foreach (var lang in _targetLanguages)
            {
                System.Diagnostics.Debug.WriteLine($"Processing translation for target language: {lang.Value}");

                // 중국어 데이터가 이미 존재하는 경우 번역을 건너뜀
                if (lang.Value == "zh-CN" && model is CategoryModel categoryModel)
                {
                    var existingChineseData = categoryModel.Locales.FirstOrDefault(l => l.LanguageId == 3);
                    if (existingChineseData != null && !string.IsNullOrWhiteSpace(existingChineseData.Name))
                    {
                        var chineseDataCopy = new TResult();
                        foreach (var prop in typeof(TResult).GetProperties())
                        {
                            if (prop.CanWrite)
                            {
                                prop.SetValue(chineseDataCopy, prop.GetValue(existingChineseData));
                            }
                        }
                        localizedModels.Add(chineseDataCopy);
                        continue;
                    }
                }

                var translatedModel = new TResult();

                // 언어 ID 설정.
                typeof(TResult).GetProperty("LanguageId")?.SetValue(translatedModel, lang.Key);

                if (baseLanguage != lang.Value)
                {
                    foreach (var property in baseProperties)
                    {
                        var originalValue = (string)property.GetValue(model);

                        // 값을 트림하여 앞뒤의 빈 공간 제거
                        originalValue = originalValue?.Trim();

                        // 값이 없으면 번역하지 않고 진행
                        if (string.IsNullOrEmpty(originalValue))
                            continue;

                        // 값이 숫자로만 이루어져 있으면 번역하지 않고 그 값을 그대로 사용
                        if (int.TryParse(originalValue, out _))
                        {
                            typeof(TResult).GetProperty(property.Name)?.SetValue(translatedModel, originalValue);
                            continue;
                        }

                        string translatedValue;

                        if (baseLanguage == lang.Value || baseLanguage == "en")
                        {
                            // 원본 언어와 목표 언어가 동일하거나 원본 언어가 영어인 경우 번역하지 않고 원본 값을 사용
                            translatedValue = originalValue;
                        }
                        else if (lang.Value != "en")
                        {
                            // 기준 언어가 영어가 아니면 먼저 영어로 번역
                            originalValue = await TranslateTextAsync(originalValue, baseLanguage, "en");
                            // 그 다음 원하는 언어로 번역
                            translatedValue = await TranslateTextAsync(originalValue, "en", lang.Value);
                        }
                        else
                        {
                            // 원본 언어를 영어로 번역
                            translatedValue = await TranslateTextAsync(originalValue, baseLanguage, "en");
                        }

                        // TResult에 해당 속성이 있는지 확인하고 설정
                        var resultProperty = typeof(TResult).GetProperty(property.Name);
                        if (resultProperty != null && resultProperty.PropertyType == typeof(string))
                        {
                            resultProperty.SetValue(translatedModel, translatedValue);
                        }
                    }
                }
                else
                {
                    foreach (var property in baseProperties)
                    {
                        var originalValue = (string)property.GetValue(model);
                        typeof(TResult).GetProperty(property.Name)?.SetValue(translatedModel, originalValue);
                    }
                }

                localizedModels.Add(translatedModel);
            }

            return localizedModels;
        }

        public class CategoryJsonModel
        {
            public string Category_id { get; set; }
            public string Category_name { get; set; }
            public List<CategoryJsonModel> Sub_categories { get; set; } = new List<CategoryJsonModel>();
            public string Updated_time { get; set; }
        }

        public class RootObject
        {
            public List<CategoryJsonModel> Categories { get; set; }
        }

        public async Task<IActionResult> GetCategoryFromTaobaoAsync()
        {
            try
            {
                _logger.Information("Fetching categories from Taobao API...");

                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://taobao-tmall-tao-bao-data-service.p.rapidapi.com/category/allCategories"),
                };

                request.Headers.Add("X-RapidAPI-Key", "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf");
                request.Headers.Add("X-RapidAPI-Host", "taobao-tmall-Tao-Bao-data-service.p.rapidapi.com");

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var rootObject = JsonConvert.DeserializeObject<RootObject>(body);

                    System.Diagnostics.Debug.WriteLine("Received categories from Taobao.");

                    var existingCategoryIds = await _categoryService.GetCategoriesByDescriptionAsync();

                    // Remove processed categories recursively
                    rootObject.Categories = rootObject.Categories.Where(c => !existingCategoryIds.Contains(c.Category_id)).ToList();
                    foreach (var category in rootObject.Categories)
                    {
                        category.Sub_categories = category.Sub_categories.Where(sc => !existingCategoryIds.Contains(sc.Category_id)).ToList();
                        foreach (var subCategory in category.Sub_categories)
                        {
                            subCategory.Sub_categories = subCategory.Sub_categories.Where(scc => !existingCategoryIds.Contains(scc.Category_id)).ToList();
                        }
                    }

                    return Ok(rootObject);
                }
            }
            catch (HttpRequestException ex)
            {
                return BadRequest($"API 호출 중 오류 발생: {ex.Message}");
            }
            catch (JsonSerializationException ex)
            {
                return BadRequest($"JSON 변환 중 오류 발생: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"서버 내부 오류: {ex.Message}");
            }
        }

        public async Task<CategoryModel> CreateCategoryModelFromTaobaoAsync(CategoryJsonModel taobaoCategory, int displayOrder, int childDisplayOrder = 0)
        {
            System.Diagnostics.Debug.WriteLine($"Creating CategoryModel from Taobao data: {taobaoCategory.Category_name} with ID: {taobaoCategory.Category_id}");

            var originalValue = taobaoCategory.Category_name;
            if (string.IsNullOrEmpty(originalValue))
            {
                throw new Exception("No categories found in Taobao data.");
            }

            var baseLanguage = await DetectLanguageAsync(originalValue);

            var translatedEn = await TranslateTextAsync(originalValue, baseLanguage, "en");
            var translatedValue = await TranslateTextAsync(translatedEn, "en", "ko");

            // CategoryModel 설정
            var model = new CategoryModel
            {
                Name = translatedValue,
                Description = taobaoCategory.Category_id,
                MetaKeywords = null,
                MetaDescription = null,
                MetaTitle = null,
                PageSizeOptions = "6,3,9",
                ParentCategoryId = 0,
                CategoryTemplateId = 1,
                PictureId = 0,
                PageSize = 5,
                AllowCustomersToSelectPageSize = true,
                ShowOnHomepage = false,
                IncludeInTopMenu = true,
                Published = true,
                Deleted = false,
                DisplayOrder = childDisplayOrder == 0 ? displayOrder : childDisplayOrder,
                PriceRangeFiltering = true,
                PriceTo = 10000,
                ManuallyPriceRange = true,
                Locales = new List<CategoryLocalizedModel>
                {
                    new CategoryLocalizedModel
                    {
                        LanguageId = 3, // 중국어의 LanguageId
                        Name = taobaoCategory.Category_name, // 원본 중국어 이름
                        Description = taobaoCategory.Category_id // 원본 카테고리 Id
                    }
                }
            };

            return model;
        }

        private async Task<int> ProcessCategoryAsync(CategoryModel model)
        {
            // 로그 추가
            _logger.Information($"Processing/Updating category: {model.Name} with Description(ID): {model.Description}");
            System.Diagnostics.Debug.WriteLine($"Processing category: {model.Name} with Description: {model.Description}");

            var existingCategory = await _categoryService.GetCategoryByDescriptionAsync(model.Description);
            Category category;

            if (existingCategory != null)
            {
                // 기존 카테고리가 존재하면 이름과 Description 변경 사항 확인
                if (existingCategory.Name != model.Name || existingCategory.Description != model.Description)
                {
                    // 변경 사항이 있을 경우에만 업데이트
                    category = model.ToEntity(existingCategory);
                    category.UpdatedOnUtc = DateTime.UtcNow;
                    await _categoryService.UpdateCategoryAsync(category);

                    // 이름이 변경되었을 경우에만 번역 작업 수행
                    if (existingCategory.Name != model.Name)
                    {
                        await UpdateLocalesAsync(category, model);
                    }
                }
                else
                {
                    // 변경 사항이 없으면 메서드 종료
                    return existingCategory.Id;
                }
            }
            else
            {
                // 존재하지 않으면 새로 생성
                category = model.ToEntity<Category>();
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryService.InsertCategoryAsync(category);
                await UpdateLocalesAsync(category, model);  // 새 카테고리를 추가할 때는 번역 작업이 필요합니다.
            }

            model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
            }

            await UpdatePictureSeoNamesAsync(category);
            await SaveCategoryAclAsync(category, model);
            await SaveStoreMappingsAsync(category, model);

            await _customerActivityService.InsertActivityAsync(existingCategory == null ? "AddNewCategory" : "EditCategory",
                string.Format(await _localizationService.GetResourceAsync(existingCategory == null ? "ActivityLog.AddNewCategory" : "ActivityLog.EditCategory"), category.Name), category);

            return category.Id;
        }

        private async Task ProcessCategoriesRecursivelyAsync(CategoryJsonModel categoryJson, int parentDisplayOrder)
        {
            // 로그 추가
            _logger.Information($"Processing main category: {categoryJson.Category_name} with ID: {categoryJson.Category_id}");
            System.Diagnostics.Debug.WriteLine($"Processing main category: {categoryJson.Category_name} with ID: {categoryJson.Category_id}");


            // 1단계 카테고리 처리
            var categoryModel = await CreateCategoryModelFromTaobaoAsync(categoryJson, parentDisplayOrder);
            var parentId = await ProcessCategoryAsync(categoryModel);

            var firstLevelChildDisplayOrder = 0;
            foreach (var firstLevelChildCategoryJson in categoryJson.Sub_categories)
            {
                // 로그 추가
                _logger.Information($"Processing 1st level child category: {firstLevelChildCategoryJson.Category_name} with ID: {firstLevelChildCategoryJson.Category_id}");
                System.Diagnostics.Debug.WriteLine($"Processing 1st level sub-category: {firstLevelChildCategoryJson.Category_name} with ID: {firstLevelChildCategoryJson.Category_id}");


                var firstLevelChildCategoryModel = await CreateCategoryModelFromTaobaoAsync(firstLevelChildCategoryJson, ++firstLevelChildDisplayOrder);
                firstLevelChildCategoryModel.ParentCategoryId = parentId;
                var firstLevelChildId = await ProcessCategoryAsync(firstLevelChildCategoryModel);

                var secondLevelChildDisplayOrder = 0;
                foreach (var secondLevelChildCategoryJson in firstLevelChildCategoryJson.Sub_categories)
                {
                    // 로그 추가
                    _logger.Information($"Processing 2nd level child category: {secondLevelChildCategoryJson.Category_name} with ID: {secondLevelChildCategoryJson.Category_id}");
                    System.Diagnostics.Debug.WriteLine($"Processing 2nd level sub-category: {secondLevelChildCategoryJson.Category_name} with ID: {secondLevelChildCategoryJson.Category_id}");

                    var secondLevelChildCategoryModel = await CreateCategoryModelFromTaobaoAsync(secondLevelChildCategoryJson, ++secondLevelChildDisplayOrder);
                    secondLevelChildCategoryModel.ParentCategoryId = firstLevelChildId;
                    var secondLevelChildId = await ProcessCategoryAsync(secondLevelChildCategoryModel);

                    var thirdLevelChildDisplayOrder = 0;
                    foreach (var thirdLevelChildCategoryJson in secondLevelChildCategoryJson.Sub_categories)
                    {
                        // 로그 추가
                        _logger.Information($"Processing 3rd level child category: {thirdLevelChildCategoryJson.Category_name} with ID: {thirdLevelChildCategoryJson.Category_id}");
                        System.Diagnostics.Debug.WriteLine($"Processing 3rd level sub-category: {thirdLevelChildCategoryJson.Category_name} with ID: {thirdLevelChildCategoryJson.Category_id}");

                        var thirdLevelChildCategoryModel = await CreateCategoryModelFromTaobaoAsync(thirdLevelChildCategoryJson, ++thirdLevelChildDisplayOrder);
                        thirdLevelChildCategoryModel.ParentCategoryId = secondLevelChildId;
                        await ProcessCategoryAsync(thirdLevelChildCategoryModel);
                    }
                }
            }
        }

        /// <summary>
        /// API를 통해 Taobao 카테고리를 가져와 NopCommerce에 카테고리를 생성합니다.
        /// </summary>
        /// <param name="continueEditing">계속 편집할지 여부를 나타냅니다.</param>
        /// <returns>카테고리 목록 뷰로 리다이렉트합니다.</returns>
        public virtual async Task<IActionResult> ApiCreate(bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var result = await GetCategoryFromTaobaoAsync();
            if (!(result is OkObjectResult okResult && okResult.Value is RootObject taobaoData))
            {
                throw new Exception("Failed to retrieve Taobao category data.");
            }

            // 데이터베이스에서 카테고리 목록 가져오기
            var existingCategoryIds = await _categoryService.GetCategoriesByDescriptionAsync();

            // 누락된 카테고리 찾기
            var missingCategories = taobaoData.Categories
                .Where(c => !existingCategoryIds.Contains(c.Category_id))
                .ToList();

            // 전역 변수로 부모 카테고리의 디스플레이 오더 관리
            var parentDisplayOrder = 48;

            // 누락된 부모 카테고리 처리
            foreach (var parentCategory in missingCategories)
            {
                await ProcessCategoriesRecursivelyAsync(parentCategory, parentDisplayOrder++);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryModelAsync(new CategoryModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity<Category>();
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryService.InsertCategoryAsync(category);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(category, model);

                //discounts
                var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                }

                await _categoryService.UpdateCategoryAsync(category);

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(category);

                //ACL (customer roles)
                await SaveCategoryAclAsync(category, model);

                //stores
                await SaveStoreMappingsAsync(category, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"), category.Name), category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = await _categoryModelFactory.PrepareCategoryModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryModelAsync(null, category);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prevPictureId = category.PictureId;

                //if parent category changes, we need to clear cache for previous parent category
                if (category.ParentCategoryId != model.ParentCategoryId)
                {
                    await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesByParentCategoryPrefix, category.ParentCategoryId);
                    await _staticCacheManager.RemoveByPrefixAsync(NopCatalogDefaults.CategoriesChildIdsPrefix, category.ParentCategoryId);
                }

                category = model.ToEntity(category);
                category.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryService.UpdateCategoryAsync(category);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(category, model.SeName, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(category, model);

                //discounts
                var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToCategories, showHidden: true, isActive: null);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new discount
                        if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is null)
                            await _categoryService.InsertDiscountCategoryMappingAsync(new DiscountCategoryMapping { DiscountId = discount.Id, EntityId = category.Id });
                    }
                    else
                    {
                        //remove discount
                        if (await _categoryService.GetDiscountAppliedToCategoryAsync(category.Id, discount.Id) is DiscountCategoryMapping mapping)
                            await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);
                    }
                }

                await _categoryService.UpdateCategoryAsync(category);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != category.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(category);

                //ACL
                await SaveCategoryAclAsync(category, model);

                //stores
                await SaveStoreMappingsAsync(category, model);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCategory"), category.Name), category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = category.Id });
            }

            //prepare model
            model = await _categoryModelFactory.PrepareCategoryModelAsync(model, category, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return RedirectToAction("List");

            await _categoryService.DeleteCategoryAsync(category);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"), category.Name), category);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            await _categoryService.DeleteCategoriesAsync(await (await _categoryService.GetCategoriesByIdsAsync(selectedIds.ToArray())).WhereAwait(async p => await _workContext.GetCurrentVendorAsync() == null).ToListAsync());

            return Json(new { Result = true });
        }

        #endregion

        #region Export / Import

        public virtual async Task<IActionResult> ExportXml()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var xml = await _exportManager.ExportCategoriesToXmlAsync();

                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "categories.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        public virtual async Task<IActionResult> ExportXlsx()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            try
            {
                var bytes = await _exportManager
                    .ExportCategoriesToXlsxAsync((await _categoryService.GetAllCategoriesAsync(showHidden: true)).ToList());

                return File(bytes, MimeTypes.TextXlsx, "categories.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportFromXlsx(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //a vendor cannot import categories
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _importManager.ImportCategoriesFromXlsxAsync(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Imported"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Products

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(CategoryProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //try to get a category with the specified id
            var category = await _categoryService.GetCategoryByIdAsync(searchModel.CategoryId)
                ?? throw new ArgumentException("No category found with the specified id");

            //prepare model
            var model = await _categoryModelFactory.PrepareCategoryProductListModelAsync(searchModel, category);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductUpdate(CategoryProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a product category with the specified id
            var productCategory = await _categoryService.GetProductCategoryByIdAsync(model.Id)
                ?? throw new ArgumentException("No product category mapping found with the specified id");

            //fill entity from product
            productCategory = model.ToEntity(productCategory);
            await _categoryService.UpdateProductCategoryAsync(productCategory);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //try to get a product category with the specified id
            var productCategory = await _categoryService.GetProductCategoryByIdAsync(id)
                ?? throw new ArgumentException("No product category mapping found with the specified id", nameof(id));

            await _categoryService.DeleteProductCategoryAsync(productCategory);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductAddPopup(int categoryId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryModelFactory.PrepareAddProductToCategorySearchModelAsync(new AddProductToCategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categoryModelFactory.PrepareAddProductToCategoryListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingProductCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(model.CategoryId, showHidden: true);
                foreach (var product in selectedProducts)
                {
                    //whether product category with such parameters already exists
                    if (_categoryService.FindProductCategory(existingProductCategories, product.Id, model.CategoryId) != null)
                        continue;

                    //insert the new product category mapping
                    await _categoryService.InsertProductCategoryAsync(new ProductCategory
                    {
                        CategoryId = model.CategoryId,
                        ProductId = product.Id,
                        IsFeaturedProduct = false,
                        DisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddProductToCategorySearchModel());
        }

        #endregion
    }
}