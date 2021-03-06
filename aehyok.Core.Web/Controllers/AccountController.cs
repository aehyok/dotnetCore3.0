﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using aehyok.Core.Data.Model;
using aehyok.Core.IRepository;
using aehyok.Core.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aehyok.Core.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountRepository accountRepository, IHttpContextAccessor httpContextAccessor, ILogger<AccountController> logger)
        {
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 登录按钮事件
        /// </summary>
        /// <param name="user"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_accountRepository.CheckLogin(user.UserName, user.Password))
                    {
                        var tempUser = new { UserName = user.UserName, Password = user.Password };
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Sid, user.Password)
                        };
                        var indentity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(indentity);

                        var authProperties = new AuthenticationProperties
                        {
                            AllowRefresh = true,
                            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10), 
                            // The time at which the authentication ticket expires. A 
                            // value set here overrides the ExpireTimeSpan option of 
                            // CookieAuthenticationOptions set with AddCookie.
                            IsPersistent = false,
                            //IssuedUtc = <DateTimeOffset>,// The time at which the authentication ticket was issued.
                            RedirectUri = returnUrl ?? "/Home/Index"
                        };
                        await _httpContextAccessor.HttpContext.SignInAsync(principal, authProperties);
                        ////return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "用户名或者密码错误,请检查。");
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
                throw exc;
            }
            return View();
        }
    }
}