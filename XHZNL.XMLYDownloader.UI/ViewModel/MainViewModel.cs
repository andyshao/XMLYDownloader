using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using XHZNL.XMLYDownloader.UI.Common;
using XHZNL.XMLYDownloader.UI.Model;
using XHZNL.XMLYDownloader.UI.Service;

namespace XHZNL.XMLYDownloader.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private MainService mainService;

        #region ����

        private string xmlyResourceUrl;

        /// <summary>
        /// ��Դ����
        /// </summary>
        public string XMLYResourceUrl
        {
            get { return xmlyResourceUrl; }
            set { xmlyResourceUrl = value; RaisePropertyChanged(() => XMLYResourceUrl); }
        }

        private ObservableCollection<XMLYResourceModel> xmlyResourceModels;

        /// <summary>
        /// ��Դ�б�
        /// </summary>
        public ObservableCollection<XMLYResourceModel> XMLYResourceModels
        {
            get { return xmlyResourceModels; }
            set { xmlyResourceModels = value; RaisePropertyChanged(() => XMLYResourceModels); }
        }

        private ObservableCollection<XMLYResourcePageModel> xmlyResourcePageModels;

        /// <summary>
        /// ��Դҳ���б�
        /// </summary>
        public ObservableCollection<XMLYResourcePageModel> XMLYResourcePageModels
        {
            get { return xmlyResourcePageModels; }
            set { xmlyResourcePageModels = value; RaisePropertyChanged(() => XMLYResourcePageModels); }
        }

        private string xmlyResourceCount;

        /// <summary>
        /// ��Դ��������
        /// </summary>
        public string XMLYResourceCount
        {
            get { return xmlyResourceCount; }
            set { xmlyResourceCount = value; RaisePropertyChanged(() => XMLYResourceCount); }
        }

        private int xmlyResourcePageSelectIndex;

        /// <summary>
        /// ѡ�е�ҳ��
        /// </summary>
        public int XMLYResourcePageSelectIndex
        {
            get { return xmlyResourcePageSelectIndex; }
            set { xmlyResourcePageSelectIndex = value; RaisePropertyChanged(() => XMLYResourcePageSelectIndex); }
        }

        /// <summary>
        /// ����Ŀ¼
        /// </summary>
        public string DownloadFolder
        {
            get
            {
                return AppConfig.DownloadFolder;
            }
            set { AppConfig.DownloadFolder = value; RaisePropertyChanged(() => DownloadFolder); }
        }

        #endregion

        #region Command

        private RelayCommand searchCommand;

        /// <summary>
        /// ������Դ����
        /// </summary>
        public RelayCommand SearchCommand
        {
            get
            {
                if (searchCommand == null)
                {
                    searchCommand = new RelayCommand(Search, () =>
                    {
                        if (string.IsNullOrWhiteSpace(XMLYResourceUrl))
                            return false;

                        return true;
                    });
                }
                return searchCommand;
            }
            set { searchCommand = value; }
        }

        /// <summary>
        /// ������Դ
        /// </summary>
        private void Search()
        {
            var result = mainService.GetXMLYResources(XMLYResourceUrl);
            XMLYResourceModels = result.Item1;
            XMLYResourcePageModels = result.Item2;
            XMLYResourceCount = result.Item3;
        }

        private RelayCommand<XMLYResourceModel> downloadCommand;

        /// <summary>
        /// ������Դ����
        /// </summary>
        public RelayCommand<XMLYResourceModel> DownloadCommand
        {
            get
            {
                if (downloadCommand == null)
                {
                    downloadCommand = new RelayCommand<XMLYResourceModel>(Download, (XMLYResourceModel p) => { return true; });
                }
                return downloadCommand;
            }
            set { downloadCommand = value; }
        }

        /// <summary>
        /// ������Դ
        /// </summary>
        /// <param name="obj"></param>
        private void Download(XMLYResourceModel obj)
        {
            obj.ShowCancelDownloadButton = true;
            obj.ShowDownloadButton = false;

            var uri = new Uri(XMLYResourceUrl);
            var rootUri = uri.AbsoluteUri.Replace(uri.AbsolutePath, "");

            var downloadUrl = mainService.GetXMLYDownloadUrl(rootUri + obj.Href);
            //var fileType = Regex.Match(downloadUrl, "[^\\.]\\w*$").Value;
            var fileType = "m4a";//Ĭ��m4a��ʽ

            mainService.DownloadFile(downloadUrl, DownloadFolder + "\\" + obj.Name + "." + fileType, p =>
              {
                  obj.DownloadProgress = p;
                  if (obj.DownloadProgress >= 100)
                  {
                      obj.ShowCancelDownloadButton = false;
                      obj.ShowDownloadButton = false;
                      obj.FileExist = true;
                  }
              });
        }

        private RelayCommand<XMLYResourceModel> cancelDownloadCommand;

        /// <summary>
        /// ȡ��������Դ����
        /// </summary>
        public RelayCommand<XMLYResourceModel> CancelDownloadCommand
        {
            get
            {
                if (cancelDownloadCommand == null)
                {
                    cancelDownloadCommand = new RelayCommand<XMLYResourceModel>(CancelDownload, (XMLYResourceModel p) => { return true; });
                }
                return cancelDownloadCommand;
            }
            set { cancelDownloadCommand = value; }
        }

        /// <summary>
        /// ȡ��������Դ
        /// </summary>
        /// <param name="obj"></param>
        private void CancelDownload(XMLYResourceModel obj)
        {
            obj.ShowCancelDownloadButton = false;
            obj.ShowDownloadButton = true;
        }

        private RelayCommand pageIndexChangedCommand;

        /// <summary>
        /// ��ҳ����
        /// </summary>
        public RelayCommand PageIndexChangedCommand
        {
            get
            {
                if (pageIndexChangedCommand == null)
                {
                    pageIndexChangedCommand = new RelayCommand(PageIndexChanged, () =>
                    {
                        if (XMLYResourcePageSelectIndex == -1)
                        {
                            //XMLYResourcePageSelectIndex = 0;
                            return false;
                        }

                        return true;
                    });
                }
                return pageIndexChangedCommand;
            }
            set { pageIndexChangedCommand = value; }
        }

        /// <summary>
        /// ��ҳ
        /// </summary>
        private void PageIndexChanged()
        {
            var pageModel = xmlyResourcePageModels[XMLYResourcePageSelectIndex];
            var uri = new Uri(XMLYResourceUrl);
            var rootUri = uri.AbsoluteUri.Replace(uri.AbsolutePath, "");
            var result = mainService.GetXMLYResources(rootUri + pageModel.Href);

            XMLYResourceModels = result.Item1;
            XMLYResourcePageModels = result.Item2;
            XMLYResourceCount = result.Item3;
        }

        private RelayCommand openFolderCommand;

        /// <summary>
        /// ��������Ŀ¼����
        /// </summary>
        public RelayCommand OpenFolderCommand
        {
            get
            {
                if (openFolderCommand == null)
                {
                    openFolderCommand = new RelayCommand(OpenFolder, () =>
                    {
                        return true;
                    });
                }
                return openFolderCommand;
            }
            set { openFolderCommand = value; }
        }

        /// <summary>
        /// ��������Ŀ¼
        /// </summary>
        public void OpenFolder()
        {
            var dialog = new CommonOpenFileDialog("����Ŀ¼");
            dialog.DefaultDirectory = DownloadFolder;
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                DownloadFolder = dialog.FileName;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            mainService = new MainService();
            //DispatcherHelper.Initialize();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                XMLYResourceModels = new ObservableCollection<XMLYResourceModel>() {
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ��һ�� ��ѧ�߽�" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� �ڶ��� ���ֺ�ũ����" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ������ Ҷ�Ľ�" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ���ļ� ������" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ���弯 �찶����" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ������ ��ʷ��ī��" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ���߼� �찶����" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� �ڰ˼� ��������" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� �ھż� �޽�" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ��ʮ�� �ۻ����˺��" },
                    new XMLYResourceModel(){ Name = "�����塷��һ�� ��ʮһ�� �����Ѿ�" },
                };

                XMLYResourcePageModels = new ObservableCollection<XMLYResourcePageModel>() {
                    new XMLYResourcePageModel(){ Num = "<" },
                    new XMLYResourcePageModel(){ Num = "1" },
                    new XMLYResourcePageModel(){ Num = "..." },
                    new XMLYResourcePageModel(){ Num = "4" },
                    new XMLYResourcePageModel(){ Num = "5" },
                    new XMLYResourcePageModel(){ Num = "6" },
                    new XMLYResourcePageModel(){ Num = "7" },
                    new XMLYResourcePageModel(){ Num = "8" },
                    new XMLYResourcePageModel(){ Num = "..." },
                    new XMLYResourcePageModel(){ Num = "50" },
                    new XMLYResourcePageModel(){ Num = ">" },
                };

                XMLYResourceCount = "ר���������(1476)";
            }
            else
            {
                // Code runs "for real"
            }
        }
    }
}