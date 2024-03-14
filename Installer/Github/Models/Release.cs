using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Installer.Github.Models;

public class Asset
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public object Label { get; set; } = string.Empty;

    [JsonPropertyName("uploader")]
    public Uploader Uploader { get; set; } = new();

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("download_count")]
    public int DownloadCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}

public class Author
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; } = string.Empty;

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; } = string.Empty;

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; } = string.Empty;

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; } = string.Empty;

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; } = string.Empty;

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; } = string.Empty;

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; } = string.Empty;

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }
}

public class Release
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("assets_url")]
    public string AssetsUrl { get; set; } = string.Empty;

    [JsonPropertyName("upload_url")]
    public string UploadUrl { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("author")]
    public Author Author { get; set; } = new();

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; } = string.Empty;

    [JsonPropertyName("target_commitish")]
    public string TargetCommitish { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; } 

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; } = [];

    [JsonPropertyName("tarball_url")]
    public string TarballUrl { get; set; } = string.Empty;

    [JsonPropertyName("zipball_url")]
    public string ZipballUrl { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

public class Uploader
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("gravatar_id")]
    public string GravatarId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;

    [JsonPropertyName("followers_url")]
    public string FollowersUrl { get; set; } = string.Empty;

    [JsonPropertyName("following_url")]
    public string FollowingUrl { get; set; } = string.Empty;

    [JsonPropertyName("gists_url")]
    public string GistsUrl { get; set; } = string.Empty;

    [JsonPropertyName("starred_url")]
    public string StarredUrl { get; set; } = string.Empty;

    [JsonPropertyName("subscriptions_url")]
    public string SubscriptionsUrl { get; set; } = string.Empty;

    [JsonPropertyName("organizations_url")]
    public string OrganizationsUrl { get; set; } = string.Empty;

    [JsonPropertyName("repos_url")]
    public string ReposUrl { get; set; } = string.Empty;

    [JsonPropertyName("events_url")]
    public string EventsUrl { get; set; } = string.Empty;

    [JsonPropertyName("received_events_url")]
    public string ReceivedEventsUrl { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("site_admin")]
    public bool SiteAdmin { get; set; }
}
