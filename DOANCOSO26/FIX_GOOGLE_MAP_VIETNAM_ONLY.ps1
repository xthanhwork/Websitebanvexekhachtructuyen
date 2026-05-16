# Chay file nay trong thu muc goc project DOANCOSO26 hoac thu muc cha chua DOANCOSO26.
# Script chi sua phan ban do, khong ghi de ca file view nen han che mat cac fix khac.

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$candidates = @(
    $scriptDir,
    (Join-Path $scriptDir "DOANCOSO26"),
    (Get-Location).Path,
    (Join-Path (Get-Location).Path "DOANCOSO26")
)

$projectRoot = $null
foreach ($c in $candidates) {
    if (Test-Path (Join-Path $c "Views\Trip\Display.cshtml")) {
        $projectRoot = $c
        break
    }
}

if (-not $projectRoot) {
    Write-Host "Khong tim thay DOANCOSO26/Views/Trip/Display.cshtml. Hay dat file ps1 trong thu muc goc DOANCOSO26 roi chay lai." -ForegroundColor Red
    exit 1
}

Write-Host "Project root: $projectRoot" -ForegroundColor Cyan

function Backup-File($path) {
    if (Test-Path $path) {
        $backup = "$path.bak_googlemap_$(Get-Date -Format 'yyyyMMddHHmmss')"
        Copy-Item $path $backup -Force
        Write-Host "Backup: $backup" -ForegroundColor DarkGray
    }
}

function Replace-Tiles-To-Google($path) {
    if (-not (Test-Path $path)) { return }
    Backup-File $path
    $content = Get-Content $path -Raw -Encoding UTF8

    # Doi cac tile OpenStreetMap sang Google Maps tile.
    $content = $content -replace "'https://\{s\}\.tile\.openstreetmap\.org/\{z\}/\{x\}/\{y\}\.png'", "'https://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}'"
    $content = $content -replace '"https://\{s\}\.tile\.openstreetmap\.org/\{z\}/\{x\}/\{y\}\.png"', '"https://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}"'

    # Them subdomains Google vao option tile neu chua co.
    if ($content -match "google\.com/vt/lyrs=m" -and $content -notmatch "subdomains:\s*\['mt0'\]") {
        $content = $content -replace "attribution:\s*'&copy;[^']*'", "attribution: '&copy; Google Maps', maxZoom: 20, subdomains: ['mt0', 'mt1', 'mt2', 'mt3']"
        $content = $content -replace 'attribution:\s*"&copy;[^\"]*"', 'attribution: "&copy; Google Maps", maxZoom: 20, subdomains: ["mt0", "mt1", "mt2", "mt3"]'
    }

    Set-Content $path $content -Encoding UTF8
    Write-Host "Da doi tile Google Maps: $path" -ForegroundColor Green
}

$displayPath = Join-Path $projectRoot "Views\Trip\Display.cshtml"
Backup-File $displayPath
$display = Get-Content $displayPath -Raw -Encoding UTF8

$googleMapScript = @'
<script>

    // Google Maps style map - mac dinh hien ban do Viet Nam
    const vietnamCenter = [16.047079, 108.206230];
    const vietnamBounds = L.latLngBounds([8.0, 102.0], [23.8, 110.0]);

    function toMapNumber(value, fallback) {
        const n = Number(String(value || '').replace(',', '.'));
        return Number.isFinite(n) ? n : fallback;
    }

    function isValidLatLng(lat, lng) {
        return Number.isFinite(lat) && Number.isFinite(lng) && Math.abs(lat) <= 90 && Math.abs(lng) <= 180;
    }

    var startLat = toMapNumber('@Model.BusRoute.StartStop?.Latitude', 10.8231);
    var startLng = toMapNumber('@Model.BusRoute.StartStop?.Longitude', 106.6297);
    var endLat = toMapNumber('@Model.BusRoute.EndStop?.Latitude', 21.0278);
    var endLng = toMapNumber('@Model.BusRoute.EndStop?.Longitude', 105.8342);

    var map = L.map('map', {
        maxBounds: vietnamBounds,
        maxBoundsViscosity: 0.35
    }).setView(vietnamCenter, 6);

    // Nen ban do kieu Google Maps
    L.tileLayer('https://{s}.google.com/vt/lyrs=m&x={x}&y={y}&z={z}', {
        maxZoom: 20,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3'],
        attribution: '&copy; Google Maps'
    }).addTo(map);

    var start = isValidLatLng(startLat, startLng) ? L.latLng(startLat, startLng) : null;
    var end = isValidLatLng(endLat, endLng) ? L.latLng(endLat, endLng) : null;

    if (start && end) {
        var bounds = L.latLngBounds([start, end]);
        map.fitBounds(bounds, { padding: [50, 50], maxZoom: 11 });

        L.marker(start)
            .addTo(map)
            .bindPopup('Điểm đi: @Model.BusRoute.StartStop?.Name');

        L.marker(end)
            .addTo(map)
            .bindPopup('Điểm đến: @Model.BusRoute.EndStop?.Name');

        fetch(`https://router.project-osrm.org/route/v1/driving/${start.lng},${start.lat};${end.lng},${end.lat}?overview=full&geometries=geojson`)
            .then(response => response.json())
            .then(data => {
                if (!data.routes || !data.routes.length) return;

                var coordinates = data.routes[0].geometry.coordinates;
                var latLngs = coordinates.map(coord => [coord[1], coord[0]]);

                L.polyline(latLngs, {
                    color: '#174ea6',
                    weight: 5,
                    opacity: 0.9
                }).addTo(map);
            })
            .catch(error => {
                console.error('Không thể lấy dữ liệu tuyến đường:', error);
            });
    } else {
        map.fitBounds(vietnamBounds);
    }

    // Icon tram dung
    var stopIcon = L.icon({
        iconUrl: '/images/position-icon.jpg',
        iconSize: [41, 41],
        iconAnchor: [12, 41],
        popupAnchor: [1, -34],
        shadowSize: [41, 41],
        shadowAnchor: [12, 41]
    });

    // Hien tram dung tren ban do
    function showStopOnMap(element) {
        var latitude = toMapNumber(element.getAttribute('data-latitude'), null);
        var longitude = toMapNumber(element.getAttribute('data-longitude'), null);

        if (!isValidLatLng(latitude, longitude)) return;

        var stopLatLng = L.latLng(latitude, longitude);

        L.marker(stopLatLng, { icon: stopIcon })
            .addTo(map)
            .bindPopup('Trạm: ' + element.querySelector('.timeline-desc span').innerText);

        map.setView(stopLatLng, 12);
    }

    // Vi tri hien tai: chi zoom neu vi tri nam trong Viet Nam, tranh ban do bi keo ra nuoc ngoai.
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function (position) {
                var currentLatLng = L.latLng(position.coords.latitude, position.coords.longitude);

                if (!vietnamBounds.contains(currentLatLng)) {
                    console.warn('Vị trí hiện tại nằm ngoài Việt Nam nên không tự chuyển bản đồ.');
                    return;
                }

                var customIcon = L.icon({
                    iconUrl: '/busjs/assets/img/logo/map-pin-svgrepo-com.png',
                    iconSize: [45, 45],
                    iconAnchor: [25, 50],
                    popupAnchor: [0, -50]
                });

                L.marker(currentLatLng, { icon: customIcon })
                    .addTo(map)
                    .bindPopup('<b>Vị trí của bạn</b>')
                    .openPopup();

                map.setView(currentLatLng, 12);
            },
            function (error) {
                console.error('Không thể xác định vị trí hiện tại:', error.message);
            }
        );
    } else {
        console.error('Trình duyệt không hỗ trợ Geolocation.');
    }

</script>
'@

# Thay rieng block map cua trang chi tiet chuyen xe, giu nguyen cac phan ghe/dat ve khac.
$pattern = '(?s)<script>\s*// Initialize map.*?</script>\s*(?=<script type="text/javascript">)'
if ([regex]::IsMatch($display, $pattern)) {
    $display = [regex]::Replace($display, $pattern, $googleMapScript + "`r`n", 1)
    Set-Content $displayPath $display -Encoding UTF8
    Write-Host "Da thay block ban do Viet Nam/Google trong Views/Trip/Display.cshtml" -ForegroundColor Green
} else {
    Write-Host "Khong tim thay block '// Initialize map' trong Display.cshtml. Script se chi doi tile OpenStreetMap sang Google neu co." -ForegroundColor Yellow
}

# Doi cac map khac trong project sang tile Google Maps, khong doi logic.
$mapFiles = @(
    "Views\Home\Index.cshtml",
    "Areas\Admin\Views\BusRoute\Add.cshtml",
    "Areas\Admin\Views\BusRoute\Display.cshtml",
    "Areas\Admin\Views\Stop\Add.cshtml"
)

foreach ($relative in $mapFiles) {
    Replace-Tiles-To-Google (Join-Path $projectRoot $relative)
}

Write-Host "Hoan tat. Hay Stop IIS Express, Clean/Rebuild, roi Ctrl+F5 tren trinh duyet." -ForegroundColor Cyan
