using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace evorace.WebApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(255)]
        public string UploadFolderName { get; set; }

        public sealed class Configuration : IEntityTypeConfiguration<ApplicationUser>
        {
            public void Configure(EntityTypeBuilder<ApplicationUser> builder)
            {
                builder.Property(x => x.UploadFolderName)
                    .HasValueGenerator<UploadFolderValueGenerator>();
            }

            internal sealed class UploadFolderValueGenerator : ValueGenerator<string>
            {
                public override bool GeneratesTemporaryValues => false;

                public override string Next([NotNull] EntityEntry entry)
                {
                    var user = (ApplicationUser)entry.Entity;
                    var email = user.NormalizedEmail;
                    var name = email.Split('@')[0].ToLower();
                    var hash = GetHash(email)[0..10];
                    var sanitizedName = mySanitizeRegex.Replace(name, "_");
                    var hashedName = $"{sanitizedName}_{hash}";

                    return hashedName;
                }

                private static string GetHash(string input)
                {
                    byte[] data = myHashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
                    var sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                    return sBuilder.ToString();
                }

                private static readonly Regex mySanitizeRegex = new Regex(@"[^a-zA-Z0-9\.]");
                // Usage is not security relevant
                private static readonly HashAlgorithm myHashAlgorithm = new SHA1CryptoServiceProvider();
            }
        }
    }
}
