using Octokit;
using PowerTools.Core.SharedServices;
using PowerTools.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace PowerTools.Helpers
{
    public static class GithubProvider
    {
        public static readonly string AppName = "PowerTools";

        public static GitHubClient GetClient(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new GitHubClient(new ProductHeaderValue(AppName));
            }

            return new GitHubClient(new ProductHeaderValue(AppName))
            {
                Credentials = new Credentials(token)
            };
        }

        public static async Task<Release> GetLatestRelease(string owner, string repo, string token)
        {
            var client = GetClient(token);
            var release = await client.Repository.Release.GetLatest(owner, repo);

            return release;
        }

        public static async Task<IReadOnlyList<Release>> GetAllReleases(string owner, string repo, string token)
        {
            var client = GetClient(token);
            var releases = await client.Repository.Release.GetAll(owner, repo);

            return releases;
        }

        public static async Task<LocalReleaseAssets> DownloadReleaseAssets(string owner, string repo, string token, Release githubRelease)
        {
            if (githubRelease == null)
                throw new ArgumentException("GitHub release cannot be null", nameof(githubRelease));

            var targetReleaseName = $"{githubRelease.Name}.zip";
            var targetReleaseName2 = $"{githubRelease.Name}.zip".Replace(" ",".");

            var targetReadmeName = $"Readme.md";
            var targetChangelogsName = $"Changelogs.md";

            var targetAsset = githubRelease.Assets.FirstOrDefault(a => a.Name.Equals(targetReleaseName, StringComparison.OrdinalIgnoreCase) || a.Name.Equals(targetReleaseName2, StringComparison.OrdinalIgnoreCase));
            var targetReadme = githubRelease.Assets.FirstOrDefault(a => a.Name.Equals(targetReadmeName, StringComparison.OrdinalIgnoreCase));
            var targetChangelogs = githubRelease.Assets.FirstOrDefault(a => a.Name.Equals(targetChangelogsName, StringComparison.OrdinalIgnoreCase));

            var tempFolder = ApplicationService.Instance.GetOrCreateTempFolder();
            var localReleaseAssets = new LocalReleaseAssets();

            if (targetAsset != null)
            {
                var savedReleasePath = Path.Combine(tempFolder, Path.GetTempFileName());
                await DownloadAsset(owner, repo, token, targetAsset.BrowserDownloadUrl, savedReleasePath);
                localReleaseAssets.ReleasePath = savedReleasePath;
            }
            if (targetReadme != null)
            {
                var savedReadmePath = Path.Combine(tempFolder, Path.GetTempFileName());
                await DownloadAsset(owner, repo, token, targetReadme.BrowserDownloadUrl, savedReadmePath);
                localReleaseAssets.ReadmePath = savedReadmePath;
            }
            if (targetChangelogs != null)
            {
                var savedChangelogsPath = Path.Combine(tempFolder, Path.GetTempFileName());
                await DownloadAsset(owner, repo, token, targetChangelogs.BrowserDownloadUrl, savedChangelogsPath);
                localReleaseAssets.ChangelogsPath = savedChangelogsPath;
            }

            return localReleaseAssets;
        }

        public static async Task DownloadAsset(string owner, string repo, string token, string onlineFilePath, string localSavePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(AppName);

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                }

                //using (var response = await client.GetAsync(apiUrl, HttpCompletionOption.ResponseHeadersRead))
                //{
                //    response.EnsureSuccessStatusCode();
                //    using (var fileStream = new FileStream(localSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                //    {
                //        await response.Content.CopyToAsync(fileStream);
                //    }
                //}

                try
                {
                    using (var downloadStream = await client.GetStreamAsync(onlineFilePath))
                    {
                        using (var fileStream = new FileStream(localSavePath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await downloadStream.CopyToAsync(fileStream);
                        }
                    }
                }
                catch (Exception e)
                {
                    LoggingService.Instance.Error($"Failed to download file {onlineFilePath}", e);
                }
            }
        }
    }
}
