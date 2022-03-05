using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tztd.Data;
using tztd.Models;
using tztd.ViewModels;

namespace tztd.Controllers {
    public class FounderController : Controller {

        /// <summary>
        /// Добавление нового учредителя
        /// </summary>
        /// <param name="model">данные учредителя</param>
        /// <returns></returns>
        [HttpGet]
        [Route ("/addfounder")]
        public async Task<JsonResult> AppendFounder (FounderModel model) {
            RequestStatus status;

            if (ModelState.IsValid) {
                using (ApplicationContext app = new ApplicationContext ()) {
                    if (await FindFounder(model) > 0) { 
                        status = new RequestStatus () { Type = Data.TypeRequest.EROOR_EXSISTS, MessageError = "Данная запись существует" };
                        return Json (status);
                    }
                    Founder newFounder = new Founder() { INN = model.INN, FullName = model.FullName };
                    app.Founders.Add (newFounder);
                    await app.SaveChangesAsync ();

                    status = new RequestStatus () { Type = Data.TypeRequest.ACCEPT, Response=newFounder, Message = "Данная запись добавлена в базу данных" };
                    return Json (status);
                }
            }

            status = new RequestStatus () { Type = Data.TypeRequest.ERROR_VALIDATION, Response = model, MessageError = "Ошибка валидации" };
            return Json (status);
        }

        /// <summary>
        /// Обновление данных существующего учредителя
        /// </summary>
        /// <param name="model">Обновляемые данные</param>
        /// <param name="id">id редактируемой записи</param>
        /// <returns></returns>
        [HttpGet]
        [Route("/editfounder")]
        public async Task<JsonResult> UpdateFounder(int id, FounderModel model) {
            RequestStatus status;

            if (ModelState.IsValid) {
                using (ApplicationContext app = new ApplicationContext()) {
                    if (await FindFounder(id)) {
                        if (!await PermisionUpdate(model, id)) { 
                            status = new RequestStatus () { Type = Data.TypeRequest.EROOR_EXSISTS, MessageError = "В системе уже есть запись с такими данными" };
                            return Json (status);
                        }

                        Founder founderUpdate = await app.Founders.FirstOrDefaultAsync(f => f.Id == id);
                        founderUpdate.UpdateData(model);
                        await app.SaveChangesAsync();

                        status = new RequestStatus () { Type = Data.TypeRequest.ACCEPT, Response=founderUpdate, Message = "Данная запись обновлена в базе данных" };
                        return Json(status);
                    }
                }
            }

            status = new RequestStatus () { Type = Data.TypeRequest.ERROR_VALIDATION, Response = model, MessageError = "Ошибка валидации" };
            return Json(status);
        }

        /// <summary>
        /// Список учредителей в системе
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/getfounders")]
        public async Task<JsonResult> GetFounders() {
            RequestStatus status;
            using (ApplicationContext app = new ApplicationContext()) {
                var founders = await app.Founders.ToListAsync();
                status = new RequestStatus () { Type = Data.TypeRequest.ACCEPT, Response = founders, Message = "Список учредителей в системе" };
                return Json(status);
            }
        }

        /// <summary>
        /// Определённый учредитель в системе
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/getfounder")]
        public async Task<JsonResult> GetFounder(int id) {
            RequestStatus status;
            using (ApplicationContext app = new ApplicationContext()) {
                if (await FindFounder(id)) {
                    Founder founder = await app.Founders.FirstOrDefaultAsync(f => f.Id == id);
                    status = new RequestStatus () { Type = Data.TypeRequest.ACCEPT, Response=founder, Message = "Искомый учредитель" };
                    return Json(status);
                }
                status = new RequestStatus () { Type = Data.TypeRequest.ERROR, MessageError = "Данного учредителя в системе не найдено" };
                return Json(status);
            }
        }

        /// <summary>
        /// Поиск пользователя в системе 
        /// </summary>
        /// <param name="model">данные искомого пользователя</param>
        /// <returns></returns>
        private async Task<int> FindFounder(FounderModel model) { 
            using(ApplicationContext app = new ApplicationContext()){
                Founder founder = await app.Founders.FirstOrDefaultAsync (f => f.INN == model.INN || f.FullName == model.FullName);
                if (founder != null)
                    return founder.Id;
            }
            return -1;
        }

        /// <summary>
        ///  Проверка на обновление данных пользователя
        /// </summary>
        /// <param name="model">Обновляемые данные</param>
        /// <param name="permissionId">id записи не участвующей в проверке</param>
        /// <returns></returns>
        private async Task<bool> PermisionUpdate(FounderModel model, int permissionId) { 
            using(ApplicationContext app = new ApplicationContext()){
                var founders = await app.Founders.Where(f => (f.INN == model.INN || f.FullName == model.FullName) && f.Id != permissionId).ToListAsync();
                return founders.Count() == 0;
            }
        }
        
        /// <summary>
        /// Поиск пользователя в системе
        /// </summary>
        /// <param name="id">id запись пользователя</param>
        /// <returns></returns>
        private async Task<bool> FindFounder(int id) { 
            using(ApplicationContext app = new ApplicationContext()){
                Founder founder = await app.Founders.FirstOrDefaultAsync (f => f.Id == id);
                if (founder != null)
                    return true;
            }
            return false;
        }
    }
}