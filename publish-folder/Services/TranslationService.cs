using Microsoft.EntityFrameworkCore;
using MyGoldenFood.ApplicationDbContext;
using MyGoldenFood.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyGoldenFood.Services
{
    public class TranslationService
    {
        private readonly AppDbContext _context;
        private readonly DeepLTranslationService _deepLService;

        public TranslationService(AppDbContext context, DeepLTranslationService deepLService)
        {
            _context = context;
            _deepLService = deepLService;
        }

        public async Task<string> GetTranslationAsync(int referenceId, string tableName, string fieldName, string language)
        {
            var translation = await _context.Translations
                .Where(t => t.ReferenceId == referenceId && 
                           t.TableName == tableName && 
                           t.FieldName == fieldName && 
                           t.Language == language)
                .Select(t => t.TranslatedValue)
                .FirstOrDefaultAsync();

            return translation ?? string.Empty;
        }

        public async Task<bool> UpdateTranslationAsync(int referenceId, string tableName, string fieldName, string language, string value)
        {
            try
            {
                var translation = await _context.Translations
                    .Where(t => t.ReferenceId == referenceId && 
                               t.TableName == tableName && 
                               t.FieldName == fieldName && 
                               t.Language == language)
                    .FirstOrDefaultAsync();

                if (translation != null)
                {
                    translation.TranslatedValue = value;
                }
                else
                {
                    translation = new Translation
                    {
                        ReferenceId = referenceId,
                        TableName = tableName,
                        FieldName = fieldName,
                        Language = language,
                        TranslatedValue = value
                    };
                    _context.Translations.Add(translation);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
