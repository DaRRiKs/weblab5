using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductClient.Models;
using ProductClient.Services;

namespace ProductClient.Controllers;

public class ProductController : Controller
{
    private readonly IProductApiService _productApiService;

    public ProductController(IProductApiService productApiService)
    {
        _productApiService = productApiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await _productApiService.GetAllAsync());
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            ViewBag.Error = ErrorMessage(ex);
            return View(Array.Empty<Product>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            return View(await _productApiService.GetByIdAsync(id));
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            ViewBag.Error = ErrorMessage(ex);
            return View("Details", (Product?)null);
        }
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        try
        {
            if (await _productApiService.CreateAsync(product))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "Не удалось добавить товар.";
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            ViewBag.Error = ErrorMessage(ex);
        }

        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _productApiService.GetByIdAsync(id);
            return product is null ? View("Details", null) : View(product);
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            ViewBag.Error = ErrorMessage(ex);
            return View("Details", null);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        product.Id = id;

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        try
        {
            if (await _productApiService.UpdateAsync(product))
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "Не удалось изменить товар.";
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            ViewBag.Error = ErrorMessage(ex);
        }

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await _productApiService.DeleteAsync(id))
            {
                TempData["Error"] = "Не удалось удалить товар.";
            }
        }
        catch (Exception ex) when (IsApiError(ex))
        {
            TempData["Error"] = ErrorMessage(ex);
        }

        return RedirectToAction(nameof(Index));
    }

    private static bool IsApiError(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or JsonException;

    private static string ErrorMessage(Exception ex) => ex switch
    {
        HttpRequestException { StatusCode: HttpStatusCode.BadRequest } => "Некорректные данные товара (400).",
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } => "Товар не найден.",
        HttpRequestException { StatusCode: HttpStatusCode.InternalServerError } => "Ошибка Web API (500).",
        HttpRequestException { StatusCode: null } => "Не удалось подключиться к Web API.",
        HttpRequestException => "Ошибка Web API.",
        TaskCanceledException => "Web API не отвечает.",
        JsonException => "Не удалось прочитать ответ Web API.",
        _ => "Ошибка обращения к Web API."
    };
}
