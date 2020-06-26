﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using NGitLab.Models;

namespace NGitLab.Impl
{
    public class ProjectClient : IProjectClient
    {
        private readonly API _api;

        public ProjectClient(API api)
        {
            _api = api;
        }

        public IEnumerable<Project> Accessible => _api.Get().GetAll<Project>(Utils.AddOrderBy(Project.Url + "?membership=true"));

        public IEnumerable<Project> Owned => _api.Get().GetAll<Project>(Utils.AddOrderBy(Project.Url + "?owned=true"));

        public IEnumerable<Project> Visible => _api.Get().GetAll<Project>(Utils.AddOrderBy(Project.Url));

        public Project this[int id] => GetById(id, new SingleProjectQuery());

        public Project Create(ProjectCreate project) => _api.Post().With(project).To<Project>(Project.Url);

        public Project this[string fullName] => _api.Get().To<Project>(Project.Url + "/" + WebUtility.UrlEncode(fullName));

        public void Delete(int id) => _api.Delete().Execute(Project.Url + "/" + id);

        private bool SupportKeysetPagination(ProjectQuery query)
        {
            return string.IsNullOrEmpty(query.Search);
        }

        public IEnumerable<Project> Get(ProjectQuery query)
        {
            var url = Project.Url;

            if (query.UserId.HasValue)
            {
                url = $"/users/{query.UserId.Value}/projects";
            }

            switch (query.Scope)
            {
                case ProjectQueryScope.Accessible:
                    url = Utils.AddParameter(url, "membership", value: true);
                    break;
                case ProjectQueryScope.Owned:
                    url = Utils.AddParameter(url, "owned", value: true);
                    break;
#pragma warning disable 618 // Obsolete
                case ProjectQueryScope.Visible:
#pragma warning restore 618
                case ProjectQueryScope.All:
                    // This is the default, it returns all visible projects.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            url = Utils.AddParameter(url, "archived", query.Archived);
            url = Utils.AddOrderBy(url, query.OrderBy, supportKeysetPagination: SupportKeysetPagination(query));
            url = Utils.AddParameter(url, "search", query.Search);
            url = Utils.AddParameter(url, "simple", query.Simple);
            url = Utils.AddParameter(url, "statistics", query.Statistics);
            url = Utils.AddParameter(url, "per_page", query.PerPage);

            if (query.Ascending == true)
            {
                url = Utils.AddParameter(url, "sort", "asc");
            }

            if (query.Visibility.HasValue)
            {
                url = Utils.AddParameter(url, "visibility", query.Visibility.ToString().ToLower());
            }

            if (query.MinAccessLevel != null)
            {
                url = Utils.AddParameter(url, "min_access_level", (int)query.MinAccessLevel.Value);
            }

            return _api.Get().GetAll<Project>(url);
        }

        public Project GetById(int id, SingleProjectQuery query)
        {
            var url = Project.Url + "/" + id;
            url = Utils.AddParameter(url, "statistics", query.Statistics);

            return _api.Get().To<Project>(url);
        }

        public Project Fork(string id, ForkProject forkProject)
        {
            return _api.Post().With(forkProject).To<Project>(Project.Url + "/" + id + "/fork");
        }

        public IEnumerable<Project> GetForks(string id, ForkedProjectQuery query)
        {
            var url = Project.Url + "/" + id + "/forks";

            if (query != null)
            {
                url = Utils.AddParameter(url, "owned", query.Owned);
                url = Utils.AddParameter(url, "archived", query.Archived);
                url = Utils.AddParameter(url, "membership", query.Membership);
                url = Utils.AddOrderBy(url, query.OrderBy);
                url = Utils.AddParameter(url, "search", query.Search);
                url = Utils.AddParameter(url, "simple", query.Simple);
                url = Utils.AddParameter(url, "statistics", query.Statistics);
                url = Utils.AddParameter(url, "per_page", query.PerPage);

                if (query.Visibility.HasValue)
                {
                    url = Utils.AddParameter(url, "visibility", query.Visibility.ToString().ToLowerInvariant());
                }

                if (query.MinAccessLevel != null)
                {
                    url = Utils.AddParameter(url, "min_access_level", (int)query.MinAccessLevel.Value);
                }
            }

            return _api.Get().GetAll<Project>(url);
        }

        public Dictionary<string, double> GetLanguages(string id)
        {
            var languages = DoGetLanguages(id);

            // After upgrading from v 11.6.2-ee to v 11.10.4-ee, the project /languages endpoint takes time execute.
            // So now we wait for the languages to be returned with a max wait time of 10 s.
            // The waiting logic should be removed once GitLab fix the issue in a version > 11.10.4-ee.
            var started = DateTime.UtcNow;
            while (!languages.Any() && (DateTime.UtcNow - started) < TimeSpan.FromSeconds(10))
            {
                Thread.Sleep(1000);
                languages = DoGetLanguages(id);
            }

            return languages;
        }

        private Dictionary<string, double> DoGetLanguages(string id)
        {
            return _api.Get().To<Dictionary<string, double>>(Project.Url + "/" + id + "/languages");
        }

        public Project Update(string id, ProjectUpdate projectUpdate)
        {
            return _api.Put().With(projectUpdate).To<Project>(Project.Url + "/" + Uri.EscapeDataString(id));
        }
    }
}