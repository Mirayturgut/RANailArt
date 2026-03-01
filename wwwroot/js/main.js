(function ($) {
    "use strict";

    // Spinner
    var spinner = function () {
        setTimeout(function () {
            if ($('#spinner').length > 0) {
                $('#spinner').removeClass('show');
            }
        }, 1);
    };
    spinner();
    
    
    // Initiate the wowjs
    new WOW().init();


    // Sticky Navbar
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.sticky-top').addClass('shadow-sm').css('top', '0px');
        } else {
            $('.sticky-top').removeClass('shadow-sm').css('top', '-100px');
        }
    });
    
    
    // Back to top button
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $('.back-to-top').fadeIn('slow');
        } else {
            $('.back-to-top').fadeOut('slow');
        }
    });
    $('.back-to-top').click(function () {
        $('html, body').animate({scrollTop: 0}, 1500, 'easeInOutExpo');
        return false;
    });


    // Modal Video
    $(document).ready(function () {
        var $videoSrc;
        $('.btn-play').click(function () {
            $videoSrc = $(this).data("src");
        });
        console.log($videoSrc);

        $('#videoModal').on('shown.bs.modal', function (e) {
            $("#video").attr('src', $videoSrc + "?autoplay=1&amp;modestbranding=1&amp;showinfo=0");
        })

        $('#videoModal').on('hide.bs.modal', function (e) {
            $("#video").attr('src', $videoSrc);
        })
    });


    // Facts counter
    $('[data-toggle="counter-up"]').counterUp({
        delay: 10,
        time: 2000
    });


    // Date and time picker
    $('.date').datetimepicker({
        format: 'L'
    });
    $('.time').datetimepicker({
        format: 'LT'
    });


    // Testimonials carousel
    $(".testimonial-carousel").owlCarousel({
        autoplay: true,
        smartSpeed: 1000,
        items: 1,
        dots: false,
        loop: true,
        nav: true,
        navText : [
            '<i class="bi bi-chevron-left"></i>',
            '<i class="bi bi-chevron-right"></i>'
        ]
    });

    
})(jQuery);
(() => {
  const form = document.getElementById("appointmentForm");
  if (!form) return;

  const msg = document.getElementById("apptMsg");

  const setMsg = (text) => { if (msg) msg.textContent = text; };

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    setMsg("Gönderiliyor...");

    const serviceName = document.getElementById("serviceName").value;
    const expert = document.getElementById("expert").value;
    const customerName = document.getElementById("customerName").value.trim();
    const customerPhone = document.getElementById("customerPhone").value.trim();
    const customerEmail = document.getElementById("customerEmail").value.trim();
    const date = document.getElementById("date").value; // YYYY-MM-DD
    const time = document.getElementById("time").value; // HH:mm
    const note = (document.getElementById("note").value || "").trim();

    if (!date || !time) {
      setMsg("Lütfen tarih ve saat seçin.");
      return;
    }

    // date+time -> ISO
    const startAtIso = new Date(`${date}T${time}:00`).toISOString();

    // Uzmanı note içine ekleyelim (DB modelinde expert yoksa şimdilik böyle)
    const finalNote = expert ? `Uzman: ${expert}\n${note}` : note;

    const payload = {
      customerName,
      customerEmail,
      customerPhone,
      serviceName,
      startAt: startAtIso,
      durationMinutes: 60,
      note: finalNote
    };

    try {
      const res = await fetch("/api/appointments", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const text = await res.text();
        setMsg(`Hata: ${res.status} - ${text}`);
        return;
      }

      const data = await res.json();
      setMsg(`✅ Talep alındı! (ID: ${data.id ?? data.Id ?? "?"})`);
      form.reset();
    } catch (err) {
      console.error(err);
      setMsg("❌ Sunucuya bağlanamadı. dotnet run açık mı?");
    }
  });
})();
