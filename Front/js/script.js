/*global $, window, Swiper*/
$(function () {
   
    "use strict";
    
    /* -- * Var Aria * -- */
    
    var nav_bar         =   $(".main_nav"),
        nav_link        =   $(".main_nav .inner_links li"),
        win_height      =   $(window).innerHeight(),
        side_bar        =   $(".links_bar"),
        nav_toggler     =   $(".main_nav .nav_toggeler"),
        inner_toggler   =   $(".inner_links .Toggler"),
        inner_links     =   $(".inner_links ul"),
        size_color      =   $(".Tools .Colors li, .Tools .Size li"),
        table_del       =   $(".Cart .Delete"),
        table_inc       =   $(".Cart .Quantity .Plus"),
        table_dec       =   $(".Cart .Quantity .Minus"),
        main_carousel   =   $(".main_carousel, .main_carousel .carousel, .main_carousel .carousel-item"),
        swiper = new Swiper('.swiper-container', {
            slidesPerView: '3',
            spaceBetween: 20,
            autoplay: {
                delay: 7000
            }
        });
    
    swiper = new Swiper('.cart_items .swiper-container', {
        slidesPerView: '4',
        spaceBetween: 20,
        autoplay: {
            delay: 7000
        }
    });
    
    /* -- Start Nice Scroll -- */
    
    $("html").niceScroll();
    
    /* -- * Set Dimensions * -- */
    
    $("body").css("padding-top", nav_bar.innerHeight());
    main_carousel.css("height", win_height - nav_bar.innerHeight());
    
    $(".check_out .check_nav li a").on("click", function (e) {
        
        e.preventDefault();
    });
    
    /* == Start Navbar Adjustment == */
    
    /* == Moving When On Click At Nav Links == */
    
    nav_link.on("click", function () {
        
        $(this).addClass("active").siblings("li").removeClass("active");
        
        $('html, body').animate({
            
            scrollTop: $("#" + $(this).data('id')).offset().top + 1
        }, 2000);
        
    });
    
    /* On Scroll Functions 
        == [ Auto Traverising Class ]
     */
    
    $(window).on('scroll', function () {
       
        $("section").each(function () {
            
            if ($(window).scrollTop() > $(this).offset().top - 300) {
                
                var sectionsId = $(this).attr('id');
                
                nav_link.removeClass('active');
                
                $(".main_nav .inner_links li[data-id='" + sectionsId + "']").addClass("active");
                
            }
            
        });
        
    });
    
    // Open - Close Side Bar
    
    nav_toggler.on("click", function () { side_bar.slideToggle(); });
    $(".User").on("click", function () { $(".user_bar").slideToggle(); });
    
    // Open - Close Inner Links In Mobile Screen
    
    inner_toggler.on("click", function () { inner_links.toggleClass("forced_open"); });
    
    // Toggle Language Flag 
    
    $(".Toggler .langs img").on("click", function () {
        
        if ($(this).attr("src") === "img/flags/sar.svg") {
            
            $(this).attr("src", "img/flags/usa.svg");
        } else {
            
            $(this).attr("src", "img/flags/sar.svg");
        }
    });
    
    // Open - Close Notification Nav
    
    $(".note").on('click', function () {
       
        $(".notifications").slideToggle();
    });
    
    /* -- * Start Carousel Functions * -- */
    
    $('.carousel').carousel({ interval: 4000, pause: false });
    
    $(".carousel-item").first().addClass("active");
    
    /* -- Start Categories Section Functions -- */
    
    $(".Cat").hover(function () {
        
        $(this).children(".info").fadeIn().css("display", "flex");
        
    }, function () {
        
        $(this).children(".info").css("display", "none");
        
    });
    
    
    /* -- * Start Card Ative Class On Size And Color * -- */
    
    size_color.on("click", function () {
       
        $(this).addClass("active").siblings("li").removeClass("active");
    });
    
    /* -- * Start Cart Functions * -- */
    
    table_del.on("click", function () {
       
        $(this).parent().parent().hide();
    });
    
    table_inc.on("click", function () {
       
        var Count   =   parseInt($(this).next().text());
        
        $(this).next().text(Count + 1);
    });
    
    table_dec.on("click", function () {
       
        var Count   =   parseInt($(this).prev().text());
        
        if (Count !== 0) {
            $(this).prev().text(Count - 1);
        }
    });
    
    /* -- Start Check Page Functions -- */
    
    $(".check_out .check_nav li").on("click", function () {
       
        $(this).addClass("active").siblings("li").removeClass("active");
        
        window.console.log($(this).attr("id"));
        
        $(".check_content section." + $(this).attr("id")).fadeIn().siblings("section").hide();
    });
    
/*------------------------------------------------------------*/
    /*------------------------------------------------------------*/
    /*------------------------------------------------------------*/
/*------------------------------------------------------------*/
});