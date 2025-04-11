using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestaurantManagement.State.Navigator;
using RestaurantManagement.Views;
using System.Windows;
using System.Windows.Input;
using RestaurantManagement.ViewModels;
using RestaurantManagement.View;

namespace RestaurantManagement.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private UserInfoViewModel userInfo;
        public UserInfoViewModel UserInfo
        {
            get => userInfo;
            set
            {
                userInfo = value;
                OnPropertyChanged(nameof(UserInfo));
            }
        }
        // Thuộc tính cho Navigator
        private Navigator navigator;
        public Navigator Navigator
        {
            get { return navigator; }
            set { navigator = value; OnPropertyChanged(); }
        }

        // Thuộc tính xác định hiển thị cho Admin hoặc Employee
        private Visibility _adminView = Visibility.Collapsed;
        public Visibility AdminView
        {
            get => _adminView;
            set
            {
                _adminView = value;
                OnPropertyChanged();
            }
        }

        private Visibility _employeeView = Visibility.Collapsed;
        public Visibility EmployeeView
        {
            get => _employeeView;
            set
            {
                _employeeView = value;
                OnPropertyChanged();
            }
        }

        private HeaderViewModel _header;
        public HeaderViewModel Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        // Command để tải Window
        public ICommand LoadWindowCommand { get; }

        // Command để đăng xuất
        public ICommand LogOutCommand { get; }
        public ICommand OpenUserInfoCommand { get; }

        public MainViewModel()
        {
            // Xử lý logic khi tải Window
            LoadWindowCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                if (p == null) return;

                p.Hide();
                LoginWindow loginWindow = new LoginWindow();
                var result = loginWindow.ShowDialog();

                if (result == true)
                {
                    var loginVM = loginWindow.DataContext as LoginWindowViewModel;
                    if (loginVM != null && loginVM.Role != -1)
                    {
                        Navigator = new Navigator(loginVM.Role);
                        UserInfo = new UserInfoViewModel(loginVM.MANV);
                        p.Show(); // Show lại MainWindow
                    }
                    else
                    {
                        p.Close(); // Chỉ đóng khi không đăng nhập thành công
                    }
                }
                else
                {
                    p.Close(); // Đóng khi user hủy đăng nhập
                }
            });


            // Xử lý logic khi đăng xuất
            LogOutCommand = new RelayCommand<Window>((p) => true, (p) =>
            {
                if (p == null)
                {
                    return;
                }
                System.Windows.Forms.Application.Restart();
                p.Close();
            });

            OpenUserInfoCommand = new RelayCommand(UserInfoWindow);
            Header = new HeaderViewModel();
        }

        private void UserInfoWindow(object? parameter)
        {
            var userInfo = new UserInfo();
            var userInfoVM = new UserInfoViewModel(UserInfo.NhanVien.MaNhanVien);

            userInfo.DataContext = userInfoVM;
            userInfo.ShowDialog();
        }
    }
}
