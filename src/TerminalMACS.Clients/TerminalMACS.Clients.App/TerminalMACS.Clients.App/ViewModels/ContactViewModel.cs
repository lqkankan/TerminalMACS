﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TerminalMACS.Clients.App.Models;
using TerminalMACS.Clients.App.Services;
using Xamarin.Forms;

namespace TerminalMACS.Clients.App.ViewModels
{
    /// <summary>
    /// 通讯录ViewModel
    /// </summary>
    public class ContactViewModel : BaseViewModel
    {
        /// <summary>
        /// 通讯录服务接口
        /// </summary>
        IContactsService _contactService;
        /// <summary>
        /// 标题
        /// </summary>
        public new string Title => "通讯录";
        private string _SearchText;
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                SetProperty(ref _SearchText, value);
            }
        }
        /// <summary>
        /// 通讯录搜索命令
        /// </summary>
        public ICommand SearchCommand { get; }
        /// <summary>
        /// 通讯录列表
        /// </summary>
        public ObservableCollection<Contact> Contacts { get; set; }
        /// <summary>
        /// 通讯录过滤列表
        /// </summary>
        public ObservableCollection<Contact> FilteredContacts
        {
            get
            {
                return string.IsNullOrEmpty(SearchText) ? Contacts
                                                        : new ObservableCollection<Contact>(Contacts?.ToList()
                                                        ?.Where(s => (!string.IsNullOrWhiteSpace(s.Name) && s.Name.ToLower().Contains(SearchText.ToLower()))
                                                        || (s.PhoneNumbers.Length > 0 && s.PhoneNumbers.ToList().Exists(cu => cu.ToString().Contains(SearchText)))));
            }
        }
        public ContactViewModel(IContactsService contactService)
        {
            _contactService = contactService;
            Contacts = new ObservableCollection<Contact>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(Contacts, null, ObservableCollectionCallback);
            _contactService.OnContactLoaded += OnContactLoaded;
            LoadContacts();


            SearchCommand = new Command(async () =>
            {
                var contacts = FilteredContacts;
                Console.WriteLine($"过滤后通讯录有：{contacts.Count}条");
                await Task.FromResult(FilteredContacts);
            });
        }

        /// <summary>
        /// BindingBase.EnableCollectionSynchronization 为集合启用跨线程更新
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="context"></param>
        /// <param name="accessMethod"></param>
        /// <param name="writeAccess"></param>
        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }

        /// <summary>
        /// 收到事件通知，读取一条通讯录信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContactLoaded(object sender, ContactEventArgs e)
        {
            Contacts.Add(e.Contact);
        }

        /// <summary>
        /// 异步读取终端通讯录
        /// </summary>
        /// <returns></returns>
        async Task LoadContacts()
        {
            try
            {
                await _contactService.RetrieveContactsAsync();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("任务已经取消");
            }
        }
    }
}
