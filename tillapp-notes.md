# TillApp — Ghi chú học WPF

---

## Host Builder (App.xaml.cs)

**Vấn đề mặc định:**
WPF tự tạo MainWindow qua `StartupUri` — không kiểm soát được quá trình khởi động, không có chỗ cài DI hay setup database.

**Giải pháp:**
Thay `StartupUri` bằng `Startup="OnStartup"` — tự viết hàm khởi động, giống `Program.cs` trong ASP.NET Core.

**`Host.CreateDefaultBuilder()`** — bộ khung của Microsoft, setup sẵn logging, configuration (appsettings.json), và DI container chỉ bằng 1 dòng.

> **MainWindow:** xóa file `MainWindow.xaml` mặc định của WPF đi, tạo lại `MainWindow.xaml` mới do mình tự viết (shell window: sidebar + content region). Host Builder lấy cái mới này từ DI ra và `.Show()` — không phải cái mặc định đã xóa.

**`.ConfigureServices()`** — nơi đăng ký mọi thứ: ViewModel, Service, DbContext, MediatR. Từ đây không cần `new` thủ công, DI tự inject qua constructor.

**`OnExit`** — gọi `StopAsync` để DI container dọn dẹp đúng thứ tự: đóng DB connection, flush log, dispose service.

**Tóm lại:** biến WPF app thành ứng dụng có vòng đời được quản lý bài bản thay vì để WPF tự làm sau lưng.

---

## MVVM

**BaseViewModel : ObservableObject** — lớp cha rỗng cho mọi ViewModel. `ObservableObject` (CommunityToolkit.Mvvm) lo việc bắn `PropertyChanged` để UI tự cập nhật khi property đổi giá trị. Để trống lúc đầu, sau này thêm field/command dùng chung (VD `IsBusy`, `Title`) thì chỉ sửa 1 chỗ, mọi ViewModel con đều có luôn.

**Property phải `public` mới bind được:** WPF binding (`{Binding X.Y}`) chỉ nhìn thấy public property, không thấy được field hay property `private`. Lỗi hay gặp: khai báo `INavigationService Navigation;` (field, mặc định private) rồi thắc mắc sao binding không chạy — phải là `public INavigationService Navigation { get; }`.

**DataContext không tự có** — phải chủ động gán. Muốn `MainWindow` binding vào `MainViewModel` thì constructor của `MainWindow` phải nhận `MainViewModel` (qua DI) rồi gán `DataContext = viewModel`. Vì `MainWindow` và `MainViewModel` đều đã đăng ký trong DI (`AddTransient`), nên khi `GetRequiredService<MainWindow>()` chạy, DI tự tạo `MainViewModel` và đưa vào constructor — không cần `new` thủ công.

**So với MVC: ViewModel giống Controller (mỏng, điều phối), không giống Service (chứa nghiệp vụ):**
- Controller (MVC): nhận request → gọi Service xử lý nghiệp vụ → trả View. Không tự viết logic nghiệp vụ, và **stateless** (mỗi request 1 instance mới, chết ngay sau khi trả response).
- ViewModel (MVVM): nhận hành động user (`[RelayCommand]`) → gọi `_mediator.Send(...)` (nghiệp vụ thật nằm ở Handler) → cập nhật property để View tự vẽ lại. Cũng không nên tự viết nghiệp vụ — y hệt nguyên tắc "Controller mỏng".
- Khác biệt quan trọng: ViewModel **có state sống lâu** suốt thời gian màn hình đang mở (VD `LoginViewModel.Username` đổi liên tục theo từng ký tự gõ) — Controller MVC không có khái niệm này vì chết ngay sau 1 request. Đây là lý do cần `[ObservableProperty]`/binding hai chiều, thứ MVC không cần.
- "Service" thật sự trong kiến trúc này nằm ở 2 chỗ: **MediatR Handler** (nghiệp vụ domain, VD `LoginCommandHandler`) và **class tên `*Service` tường minh** (hạ tầng UI, VD `INavigationService`, `ISessionService`).

---

## CQRS / MediatR

**⚠️ Lưu ý license:** từ 1 bản version nào đó, `MediatR` (package gốc trên NuGet) đã chuyển sang mô hình thương mại (công ty Lucky Penny Software mua lại từ tác giả Jimmy Bogard) — dùng ở production cần trả phí, chỉ free cho dev/test. Đang ghim ở bản **`12.4.1`** — bản MIT license miễn phí cuối cùng trước khi đổi mô hình. API gần như không đổi so với bản mới. Nhớ đừng để ai (hoặc Visual Studio) tự "Update NuGet package" lên bản mới hơn mà không để ý.

**Ý tưởng:** mỗi hành động (login, đặt hàng, ping...) là 1 class riêng (`IRequest<TResult>`) + 1 `Handler` xử lý nó (`IRequestHandler<TRequest, TResult>`). ViewModel không gọi thẳng Service/DbContext — chỉ gọi `_mediator.Send(new XxxCommand(...))`, MediatR tự tìm đúng Handler để chạy. Tách nhỏ theo use-case thay vì gom hết vào 1 "Service" khổng lồ.

**Đăng ký 1 dòng, không cần khai báo tay từng Handler:**
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<App>());
```
Dòng này quét cả assembly, tự tìm mọi class implement `IRequestHandler<,>` rồi đăng ký hết vào DI.

**Command vs kết quả:**
```csharp
public record LoginCommand(string Username, string Password) : IRequest<LoginResult>;
public record LoginResult(bool Success, UserRole Role, string? ErrorMessage);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    public Task<LoginResult> Handle(LoginCommand cmd, CancellationToken ct) { ... }
}
```
`record` hợp vì Command chỉ là gói dữ liệu bất biến (input), không có behavior.

**Test pipeline bằng `PingCommand` trước khi làm feature thật** — Command không cần input (`record PingCommand : IRequest<string>`), Handler trả thẳng `"Pong"`. Gọi thử từ ViewModel để chắc chắn: ViewModel → Mediator → Handler → trả kết quả ngược lại chạy thông suốt, trước khi build Login/CRUD phức tạp hơn — dễ debug hơn nhiều nếu lỗi xảy ra sau này chỉ nằm ở logic nghiệp vụ, không phải ở việc setup DI/MediatR.

**Constructor không `async` được** — nên Command đầu tiên (như Ping, hay gọi Login lúc khởi động) phải gọi từ 1 method `async Task InitializeAsync()` riêng, gọi method này ngay sau khi tạo ViewModel (VD trong code-behind của View/Window, sau `DataContext = viewModel;`).

**`ValidationBehavior` — chặn nghiệp vụ trước khi Handler chạy, không cần `if` rải rác:**
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // chạy hết validator khớp TRequest, throw ValidationException nếu có lỗi, không thì next()
    }
}
// đăng ký: cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
```
Đây là **Pipeline Behavior** — MediatR cho phép chèn code chạy **trước/sau mọi Handler** mà không cần sửa từng Handler. `IEnumerable<IValidator<TRequest>>` rỗng (Command chưa có Validator nào, VD `PingCommand`) thì bỏ qua, chạy thẳng `next()` — không bắt buộc Command nào cũng phải có Validator.

FluentValidation cho validator inject thẳng `AppDbContext` để check nghiệp vụ cần query DB (VD "xuất kho không được vượt tồn hiện có"), không chỉ check dữ liệu tĩnh trong Command:
```csharp
public class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator(AppDbContext db)
    {
        RuleFor(x => x.Qty).GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
        RuleFor(x => x).MustAsync(async (cmd, ct) => { /* query db kiểm tra tồn kho */ })
            .WithMessage("Không đủ tồn kho để xuất");
    }
}
```

**`throw` (Behavior) và `catch` (ViewModel) là 2 trách nhiệm tách biệt, không gộp 1 chỗ:**
- `ValidationBehavior` chỉ *ném* `ValidationException` — không biết, không được phép biết cách hiện lỗi ra UI (`MessageBox` thuộc `System.Windows`, tầng nghiệp vụ không nên đụng tới).
- Nơi gọi `_mediator.Send(...)` (ViewModel) phải tự `catch` nếu muốn báo lỗi cho người dùng — không catch thì exception bay tiếp lên trên, tuỳ tình huống mà app crash hoặc nút bấm "im lặng không phản ứng gì", người dùng không hiểu vì sao thao tác thất bại.
```csharp
try
{
    await _mediator.Send(new CheckoutCommand(items, "Cash"));
}
catch (ValidationException ex)
{
    MessageBox.Show(string.Join("\n", ex.Errors.Select(e => e.ErrorMessage)), "Không thể thanh toán");
    return;
}
```
Mỗi ViewModel tự quyết định "lỗi này nghĩa là gì trên màn hình của tôi" (MessageBox, đổi màu ô nhập...) — validator/handler không nên biết chuyện đó.

**`CheckoutCommand` — nhiều bảng thay đổi cùng lúc, chỉ 1 `SaveChangesAsync()` để đảm bảo atomic:** tạo `Order` + từng `OrderItem` + `StockMovement` (xuất kho) + trừ `Product.CurrentStock`, tất cả gom vào 1 lần `SaveChangesAsync()` duy nhất trong `CheckoutHandler`. Nhắc lại nguyên tắc đã ghi ở mục EF Core: **không** gọi lại `_mediator.Send(new AdjustStockCommand(...))` từ trong `CheckoutHandler` dù muốn tái dùng code — vì `AdjustStockHandler` tự `SaveChangesAsync()` riêng, gọi nhiều lần (mỗi sản phẩm 1 lần) nghĩa là nhiều transaction rời rạc, lỗi giữa chừng sẽ để lại dữ liệu nửa vời (đã trừ kho nhưng chưa có đơn hàng). Chấp nhận lặp lại vài dòng logic trừ kho trực tiếp trong `CheckoutHandler` — atomicity quan trọng hơn tránh lặp code ở đây.

**Voucher — 3 lỗi kinh điển khi viết validate bằng tay, tự kiểm code mới thấy:**
```csharp
if (voucher.ExpiresAt >= DateTime.Now) return ...; // ❌ ExpiresAt >= now nghĩa là CÒN hạn, không phải hết hạn
if (voucher.UsedCount > voucher.MaxUsage) return ...; // ❌ off-by-one: UsedCount == MaxUsage vẫn lọt qua
public async Task<VoucherApplyResult> Handler(...)  // ❌ interface yêu cầu "Handle", thừa 1 chữ "r" là lỗi build ngay
```
Bài học: điều kiện hết hạn/giới hạn rất dễ viết ngược dấu (`<` vs `>=`) hoặc lệch 1 đơn vị (`>` vs `>=`) — loại lỗi này build vẫn sạch, chỉ lộ ra khi test đúng ca biên (VD dùng đúng lần thứ `MaxUsage`, hoặc test đúng ngày hết hạn) — không test kỹ thì âm thầm cho qua case lẽ ra phải chặn.

**Số hiển thị trên UI phải là chính số được lưu vào DB, không phải 2 phép tính riêng biệt:** `SalesViewModel.GrandTotal` trừ `VoucherDiscount` để hiện cho thu ngân xem — nhưng `CheckoutHandler` (Handler tạo `Order`) ban đầu tính `Total` thẳng từ danh sách sản phẩm, **không hề biết** có voucher hay không. Hậu quả: khách được giảm giá, thu ngân thu đúng số tiền đã giảm (vì `PaymentDialog` nhận đúng `GrandTotal`), nhưng **đơn hàng lưu trong DB lại ghi số tiền chưa giảm** — sổ sách sai lệch với tiền mặt thực thu. Sửa bằng cách gửi luôn `VoucherDiscount` vào `CheckoutCommand`, trừ ngay trong `CheckoutHandler` khi tính `Order.Total` — đảm bảo **chỉ có 1 công thức tính tổng tiền**, không tính rải rác ở nhiều nơi rồi hy vọng chúng luôn khớp nhau.

**`BulkImportProductsCommand(string FilePath)` — Command nhận đường dẫn file, không nhận sẵn `List<Row>` đã parse:**
```csharp
public record BulkImportProductsCommand(string FilePath) : IRequest<BulkImportResult>;
public record ImportError(int Line, string Message);
public record BulkImportResult(int SuccessCount, int FailCount, List<ImportError> Errors);
```
ViewModel chỉ lo việc mở `OpenFileDialog` lấy đường dẫn rồi gửi Command — toàn bộ việc đọc/parse CSV (`CsvHelper`) nằm trong Handler. Giữ ViewModel mỏng, và logic đọc file (dùng `CsvConfiguration(CultureInfo.InvariantCulture)` để tránh lỗi parse số khi máy để định dạng số kiểu Việt Nam) là nghiệp vụ, thuộc về Handler.

**Tránh N+1 query khi validate từng dòng CSV — load sẵn 1 lần trước vòng lặp:**
```csharp
var categories = await _db.Categories.ToDictionaryAsync(c => c.Name, c => c.Id, ct);
var seenBarcodes = new HashSet<string>(await _db.Products.Select(p => p.Barcode).ToListAsync(ct));
```
Nếu để trong vòng `foreach` mà mỗi dòng CSV tự query DB kiểm tra category/barcode trùng, file 1000 dòng sẽ bắn ra hàng nghìn query nhỏ — chậm. Load 1 lần thành `Dictionary`/`HashSet`, sau đó check trong bộ nhớ (`TryGetValue`, `.Add()` trả `false` nếu đã tồn tại) — vừa nhanh vừa tiện phát hiện luôn barcode trùng *ngay trong chính file CSV đang import* (không chỉ trùng với DB).

**`ImportError(int Line, string Message)`** — báo lỗi kèm số dòng để người dùng dò lại trong Excel, thay vì chỉ nói chung chung "import thất bại".

---

## Data Binding

**`PasswordBox.Password` không bind trực tiếp được** — WPF cố tình không cho `Password` là `DependencyProperty` vì lý do bảo mật (tránh lộ mật khẩu qua binding/memory dump dễ dàng). Cách xử lý ở mức học/thực hành: bắt event `PasswordChanged` trong code-behind, gán thủ công qua ViewModel:
```csharp
private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
{
    if (DataContext is LoginViewModel vm)
        vm.Password = PasswordBox.Password;
}
```
Đây là ngoại lệ hiếm hoi chấp nhận code trong code-behind dù đang theo MVVM — cách "sạch" hơn là viết Attached Property/Behavior riêng, để sau khi quen.

**`UpdateSourceTrigger=PropertyChanged`** — mặc định `TextBox.Text` chỉ đẩy giá trị vào ViewModel khi mất focus (`LostFocus`). Muốn ViewModel cập nhật ngay từng ký tự gõ (để validate real-time, enable/disable nút...) thì phải khai báo rõ:
```xml
<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"/>
```

**`DataTemplate` là cầu nối ViewModel ↔ View** — khai báo trong `App.xaml` (`Application.Resources`):
```xml
<DataTemplate DataType="{x:Type vm:LoginViewModel}">
    <views:LoginView />
</DataTemplate>
```
Khi 1 `ContentControl` có `Content` là instance kiểu `LoginViewModel`, WPF tự tìm `DataTemplate` khớp `DataType`, vẽ `LoginView` lên và tự gán `DataContext` của `LoginView` = chính instance đó — không cần code gán tay. Chưa có `DataTemplate` thì `ContentControl` chỉ hiện tên class (`TillApp.ViewModels.LoginViewModel`) vì nó gọi `.ToString()` mặc định.

**`DataTrigger` — đổi style theo dữ liệu, không cần code-behind hay converter:**
```xml
<DataGridTextColumn.CellStyle>
    <Style TargetType="DataGridCell">
        <Style.Triggers>
            <DataTrigger Binding="{Binding IsLowStock}" Value="True">
                <Setter Property="Foreground" Value="Red"/>
            </DataTrigger>
        </Style.Triggers>
    </Style>
</DataGridTextColumn.CellStyle>
```
"Nếu `IsLowStock == true` thì đổi màu chữ" — thuần XAML. Điểm quan trọng: `IsLowStock` nên **tính sẵn trong Handler** (C#, dễ test/đổi công thức) rồi trả ra DTO dạng `bool`, không nên tính toán nghiệp vụ ("thế nào là sắp hết hàng") ngay trong XAML bằng `IValueConverter` — XAML chỉ nên quyết định *hiển thị*, không quyết định *nghiệp vụ*.

**`RelativeSource AncestorType` — gọi Command của ViewModel cha từ bên trong `DataTemplate`:**
```xml
<ItemsControl ItemsSource="{Binding Products}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Button Command="{Binding DataContext.AddToCartCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                    CommandParameter="{Binding}"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```
Bên trong `DataTemplate` của `ItemsControl`/`ListView`, `DataContext` của mỗi `Button` tự động đổi thành **từng item** trong list (ở đây là 1 `ProductDto`), không còn là `SalesViewModel` nữa — nên `{Binding AddToCartCommand}` thường sẽ không tìm thấy gì (vì `ProductDto` không có `AddToCartCommand`). `RelativeSource={RelativeSource AncestorType=UserControl}` là cách "trèo ngược lên" tìm `DataContext` của `UserControl` cha (chính là `SalesViewModel`) để lấy đúng Command. `CommandParameter="{Binding}"` (không ghi property con) nghĩa là truyền **nguyên cả item** (`ProductDto`/`CartItemViewModel`) đang render làm tham số cho Command.

**`[NotifyPropertyChangedFor]` — tự bắn `PropertyChanged` của 1 property tính toán khi property khác đổi:**
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(LineTotal))]
private int _qty = 1;

public decimal LineTotal => (UnitPrice * Qty) - LineDiscount;
```
`LineTotal` không có field riêng, chỉ là phép tính từ `Qty`/`LineDiscount` — nhưng WPF không tự biết "khi `Qty` đổi thì `LineTotal` cũng đổi theo" trừ khi khai rõ bằng `NotifyPropertyChangedFor`. Thiếu dòng này, đổi `Qty` sẽ không làm `TextBlock` hiện `LineTotal` cập nhật, dù giá trị thật sự đã đổi.

**Tổng tiền giỏ hàng (`GrandTotal`) không tự cập nhật khi 1 item con trong `ObservableCollection` đổi giá trị** — `ObservableCollection` chỉ báo khi **thêm/bớt phần tử**, không báo khi **1 phần tử bên trong tự đổi property** (VD `CartItemViewModel.LineTotal` đổi vì bấm +/-). Phải tự đăng ký lắng nghe từng item:
```csharp
item.PropertyChanged += CartItem_PropertyChanged;
// ...
private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(CartItemViewModel.LineTotal))
        OnPropertyChanged(nameof(GrandTotal));
}
```
Nhớ gỡ đăng ký (`-=`) khi xóa item khỏi giỏ hàng (`RemoveFromCart`) — không gỡ sẽ gây **memory leak nhỏ** (item bị xóa khỏi UI nhưng `SalesViewModel` vẫn giữ tham chiếu tới nó qua event, không bao giờ được garbage collector dọn).

**`ItemsControl` + `WrapPanel` (không phải `DataGrid`) khi hiển thị dạng lưới card, không phải bảng:** `DataGrid` (dùng ở `ProductListView`) hợp khi cần **cột/hàng rõ ràng, sort, resize cột**. Màn hình bán hàng cần **card sản phẩm dạng lưới, tự xuống dòng** (ảnh + tên + giá trong 1 ô vuông) — dùng `ItemsControl` (chỉ lặp `ItemTemplate` cho từng item, không có khái niệm cột) với `ItemsPanel` là `WrapPanel` (tự xuống dòng khi hết chỗ ngang) sẽ đúng ý hơn nhiều so với ép `DataGrid` hiển thị dạng lưới.

**`RadioButton` nhóm chọn 1 trong nhiều — không bind thẳng vào `string`/`enum` được, phải qua code-behind:** `RadioButton.IsChecked` là kiểu `bool`, còn property muốn lưu kết quả (VD `SelectedMethod`) là `string` — 2 kiểu khác nhau, không thể `{Binding SelectedMethod}` trực tiếp như `TextBox.Text`. Cách đơn giản (không cần viết `IValueConverter`): bắt sự kiện `Checked` của từng `RadioButton`, tự gán property trong code-behind:
```xml
<RadioButton Content="Tiền mặt" GroupName="PaymentMethod" IsChecked="True" Checked="CashRadio_Checked"/>
<RadioButton Content="Chuyển khoản" GroupName="PaymentMethod" Checked="TransferRadio_Checked"/>
```
```csharp
private void CashRadio_Checked(object sender, RoutedEventArgs e)
{
    if (DataContext is PaymentDialogViewModel vm) vm.SelectedMethod = "Cash";
    if (CashPanel != null) CashPanel.Visibility = Visibility.Visible;
}
```
**Bẫy cần nhớ:** `RadioButton` có `IsChecked="True"` khai sẵn trong XAML sẽ bắn sự kiện `Checked` **ngay trong lúc `InitializeComponent()` đang chạy** — tại thời điểm đó, các control khai *sau* trong cây XAML (VD `CashPanel`) **có thể chưa được gán vào field** (`x:Name`) xong. Thiếu `if (CashPanel != null)` sẽ ném `NullReferenceException` ngay khi mở dialog, dù code nhìn qua tưởng chạy sau khi UI đã dựng xong hoàn chỉnh.

**Máy quét mã vạch USB = bàn phím ảo, gõ ký tự rồi tự bấm Enter** — không cần driver/API riêng để "đọc" máy quét. Chỉ cần 1 `TextBox` luôn giữ focus, bắt sự kiện `KeyDown` chờ phím `Enter`:
```csharp
private async void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
{
    if (e.Key == Key.Enter && DataContext is SalesViewModel vm)
    {
        await vm.ScanBarcodeCommand.ExecuteAsync(null);
        BarcodeTextBox.Focus(); // quét xong lại focus ngay cho lần quét tiếp theo
    }
}
```
Đây là 1 trong số ít chỗ **chấp nhận code trong code-behind** dù theo MVVM — `KeyDown` là sự kiện UI thuần túy (không có "Command" tương đương sẵn cho phím Enter trên `TextBox` như `Button.Command`), xử lý ở code-behind rồi gọi thẳng `vm.ScanBarcodeCommand.ExecuteAsync(null)` là cách gọn nhất. Gọi `.Focus()` lại ngay sau khi xử lý xong để thu ngân quét liên tục không cần bấm chuột vào ô nhập.

---

## NavigationService

**Vấn đề:** chuyển màn hình bằng `new Window()` mỗi lần rất tốn — mất state của MainWindow, mở nhiều cửa sổ lung tung, không giống app POS thực tế (menu bên trái, nội dung đổi bên phải).

**Giải pháp:** không tạo Window mới, chỉ đổi **ViewModel** đang được hiển thị.
- `INavigationService` có property `CurrentViewModel` + method `NavigateTo<TViewModel>()`.
- `NavigateTo<T>()` gọi `_serviceProvider.GetRequiredService<T>()` để DI tạo ViewModel đích, rồi gán vào `CurrentViewModel`.
- `CurrentViewModel` đánh dấu `[ObservableProperty]` → tự bắn `PropertyChanged` khi đổi.
- `MainWindow.xaml` có `<ContentControl Content="{Binding Navigation.CurrentViewModel}"/>` — khi `CurrentViewModel` đổi, WPF tự vẽ lại View tương ứng.

**DataTemplate map ViewModel → View:** đã làm ở bước Login — xem chi tiết cơ chế ở mục [Data Binding](#data-binding) bên dưới.

**`IDialogService` — cùng tinh thần `INavigationService`, nhưng cho màn hình dạng cửa sổ nổi (modal) thay vì trang điều hướng bên trong `MainWindow`:**
- `NavigationService` đổi `CurrentViewModel` để `ContentControl`/`DataTemplate` vẽ lại — dùng cho các trang **không chặn thao tác** màn chính (Login, Sản phẩm, Bán hàng).
- `IDialogService` dùng cho popup **chặn thao tác cho tới khi đóng** (PaymentDialog) — về bản chất vẫn là `Window` thật, gọi `ShowDialog()` chứ không phải `Show()`. Điểm chung với `NavigationService`: **ViewModel gọi service (`IDialogService`), không tự `new Window()` trực tiếp** — giữ ViewModel không phụ thuộc vào 1 class UI cụ thể.
```csharp
public interface IDialogService
{
    PaymentResult? ShowPaymentDialog(decimal total);
}

public class DialogService : IDialogService
{
    public PaymentResult? ShowPaymentDialog(decimal total)
    {
        var vm = new PaymentDialogViewModel(total);          // new trực tiếp, không qua DI
        var window = new PaymentDialogView { DataContext = vm };

        var ok = window.ShowDialog();                        // block tới khi đóng
        if (ok != true || !vm.Confirmed) return null;         // null = user hủy/đóng dialog

        return new PaymentResult(true, vm.SelectedMethod, vm.CashReceived);
    }
}
```

**Vì sao `PaymentDialogViewModel` không đăng ký `AddTransient` như ViewModel khác:** `total` (tổng tiền giỏ hàng) là **dữ liệu runtime**, chỉ biết lúc bấm "Thanh toán", không phải dependency cố định như `IMediator`/`INavigationService` — DI container không biết trước giá trị này để inject. Nên tạo `new PaymentDialogViewModel(total)` trực tiếp trong `DialogService`, không qua `GetRequiredService`.

**`ShowDialog()` chỉ biết `Window.DialogResult`, không tự đọc property của ViewModel** — vì vậy `PaymentDialogViewModel` cần thêm `public bool Confirmed { get; private set; }` riêng, và code-behind của `PaymentDialogView` phải tự set `DialogResult = true;` rồi `Close()` sau khi xác nhận hợp lệ. Trả `null` (không phải `Confirmed = false`) khi hủy để phân biệt rõ "chưa quyết định" với "quyết định không xác nhận".

---

## EF Core + SQLite

**SQLite là database nhúng thẳng vào app** — không phải server riêng như SQL Server/PostgreSQL. Cả DB chỉ là 1 file (`tillapp.db`) app đọc/ghi trực tiếp, không qua network. Hợp app POS chạy desktop: không cần cài đặt gì thêm, chạy offline hoàn toàn, backup = copy file. Giới hạn cần nhớ: nhiều máy tính tiền **không nên share chung 1 file `.db` qua mạng** (dễ lock khi ghi đồng thời) — kiến trúc đúng là mỗi máy có SQLite riêng, đồng bộ lên server qua REST API sau (Phase 5).

**`AppDbContext` cần đủ 2 thứ mới hoạt động được qua DI:**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; } = default!;
}
```
- Constructor nhận `DbContextOptions<AppDbContext>` — thiếu cái này thì cấu hình `UseSqlite(...)` khai ở `AddDbContext` không bao giờ tới được instance thật, lỗi "No database provider has been configured" lúc chạy.
- `DbSet<T>` phải `public` — EF Core dùng reflection tự dò tìm, để `private` thì EF Core coi như không có bảng này.

**Navigation property = khóa ngoại, theo convention không cần cấu hình thêm:**
```csharp
public class Product
{
    public int CategoryId { get; set; }      // convention: <TênBảng>Id → tự hiểu là FK
    public Category Category { get; set; } = null!;  // navigation property
}
```

**`.Include()` — EF Core mặc định KHÔNG tự load navigation property** (khác vài ORM có lazy-loading mặc định). Thiếu `.Include(p => p.Category)` thì `p.Category.Name` ném `NullReferenceException` vì `Category` chưa được nạp từ DB.

**Migrations vs `EnsureCreated()`:**
- `EnsureCreated()` — tạo bảng nhanh theo model hiện tại, **không** ghi lại lịch sử thay đổi schema. Dùng tạm lúc mới học, ít bảng.
- Migrations (`dotnet ef migrations add X` + `Database.Migrate()`) — track từng thay đổi schema theo thời gian, cho phép sửa sau mà không mất data cũ. **2 cách này không dùng chung được** — chuyển từ `EnsureCreated()` sang Migrations giữa chừng phải xóa DB cũ, vì EF Core không biết bảng nào "đã có" nếu thiếu bảng lịch sử `__EFMigrationsHistory`.

**Upsert pattern — khi 1 bảng chỉ có đúng 1 dòng (VD `StoreSettings`):**
```csharp
var settings = await _db.StoreSettings.FirstOrDefaultAsync();
if (settings is null) { settings = new StoreSettings(); _db.StoreSettings.Add(settings); }
settings.Name = cmd.Name; // ... gán các field khác
await _db.SaveChangesAsync();
```
"Có thì sửa, chưa có thì tạo" gộp vào 1 Command — tách riêng Create/Update không có ý nghĩa với bảng chỉ 1 dòng.

**Nguyên tắc "chỉ 1 đường sửa dữ liệu quan trọng":** cột `Product.CurrentStock` **chỉ được sửa qua `AdjustStockCommand`**, không bao giờ set trực tiếp ở `CreateProduct`/`UpdateProduct`. Mỗi lần đổi tồn kho đều tạo kèm 1 bản ghi `StockMovement` (log ai/khi nào/vì sao) — nếu cho phép nhiều Command cùng sửa 1 cột, sau này không thể truy vết lý do số liệu sai.

**1 `SaveChangesAsync()` cho nhiều thay đổi = 1 transaction:** `AdjustStockCommand` vừa `Add` StockMovement vừa sửa `product.CurrentStock`, chỉ gọi `SaveChangesAsync()` 1 lần — EF Core tự gộp mọi thay đổi tracked thành 1 transaction DB, hoặc cùng thành công hoặc cùng rollback. Không cần tự viết `BeginTransaction()` cho trường hợp đơn giản này.

**Seed data — chỉ chạy 1 lần lúc DB còn trống:**
```csharp
if (await db.Categories.AnyAsync()) return; // đã có data rồi thì thôi
```
Gọi ngay sau `Database.Migrate()` lúc khởi động app — tránh chèn trùng dữ liệu mẫu mỗi lần mở app.

---

## Dependency Injection — Lifetime

**3 lifetime cơ bản:**
- `AddTransient` — mỗi lần `GetRequiredService<T>()` (hoặc DI tự inject) là 1 instance mới toanh.
- `AddScoped` — 1 instance dùng chung trong cùng 1 "scope" (VD 1 HTTP request trong ASP.NET Core), hết scope thì dispose, scope sau lại có instance khác.
- `AddSingleton` — chỉ 1 instance duy nhất cho suốt vòng đời app, ai cũng dùng chung.

**Case thật gặp phải: giỏ hàng biến mất khi chuyển màn hình qua lại.** `SalesViewModel` ban đầu đăng ký `AddTransient`. `NavigationService.NavigateTo<T>()` gọi `GetRequiredService<T>()` mỗi lần điều hướng — với `Transient`, mỗi lần bấm nút sidebar là **1 instance `SalesViewModel` mới**, `CartItems` rỗng lại từ đầu. Thu ngân bán dở giỏ hàng, lỡ bấm qua "Sản phẩm" rồi quay lại "Bán hàng" là mất sạch giỏ. Sửa bằng cách đổi sang `AddSingleton` — giữ đúng 1 instance suốt vòng đời app, giỏ hàng không mất khi chuyển màn hình. Đánh đổi: phải tự tay `CartItems.Clear()` sau khi checkout xong (Singleton không tự "sạch" như Transient mỗi lần tạo mới).

**Bài học chọn lifetime:** không phải chi tiết kỹ thuật vô hại — chọn sai ảnh hưởng thẳng tới nghiệp vụ (ở đây là mất dữ liệu đang thao tác dở).

**Case thứ 2, tinh vi hơn: `Scoped` âm thầm biến thành `Singleton` nếu không tạo `scope` đúng chỗ.** `AppDbContext` đăng ký qua `AddDbContext<AppDbContext>(...)` — mặc định lifetime là **Scoped**. Nhưng app WPF chỉ tạo `scope` tường minh **đúng 1 chỗ** (lúc `Database.Migrate()`/seed trong `OnStartup`, dùng `_host.Services.CreateScope()`) — mọi ViewModel/Handler khác đều được resolve thẳng từ **root container** (`_host.Services...`), không qua scope nào cả.

Bình thường, resolve 1 service `Scoped` thẳng từ root container sẽ bị chặn (lỗi "Cannot resolve scoped service from root provider") — **NHƯNG** lỗi này chỉ được bật khi `ServiceProviderOptions.ValidateScopes = true`, và `Host.CreateDefaultBuilder()` chỉ tự bật validate này khi môi trường là **Development**. App chạy môi trường mặc định **Production** (không set biến `DOTNET_ENVIRONMENT`) → validate tắt → không báo lỗi, nhưng service `Scoped` đó bị **cache lại trong root container y hệt 1 Singleton**, sống suốt đời app thay vì "mỗi scope 1 instance" như đúng nghĩa `Scoped`.

Hệ quả: `AppDbContext` (được khai Scoped) thật ra đang chạy như **Singleton âm thầm** suốt app — EF Core change tracker không bao giờ được dọn/reset, tích tụ theo thời gian chạy dài có thể gây rò rỉ bộ nhớ hoặc giữ dữ liệu cũ (stale). Không lỗi ngay lập tức, nhưng là kiểu bug "im lặng" nguy hiểm — set đúng lifetime (`AddDbContext` mặc định Scoped) không có nghĩa là chạy đúng như khai, nếu cách resolve service (có tạo `scope` hay không) không khớp với lifetime đã chọn.

**Việc cần làm sau này (chưa sửa vội):** cân nhắc tạo `scope` mới cho mỗi "đơn vị công việc" hợp lý (VD mỗi lần `NavigateTo` một màn hình mới, hoặc mỗi lần `_mediator.Send()`) thay vì resolve mọi thứ từ root — hoặc đơn giản hơn cho app quy mô nhỏ: chấp nhận `AppDbContext` sống như Singleton nhưng chủ động gọi `ChangeTracker.Clear()` định kỳ để tránh tích tụ.
